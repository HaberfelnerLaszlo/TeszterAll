using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeszterLibrary.Services
{
    /// <summary>
    /// Közös HTTP-logika a Teszter és Feladat API-khoz. Itt van egyetlen helyen:
    /// a kérés összeállítása, a válasz-boríték (MainResponse) felismerése, a hibaüzenetek
    /// előállítása és a kivételkezelés. A leszármazottak csak az URL-t, a hitelesítést
    /// és a típusos Get/Post/Put/Delete burkolókat adják.
    /// </summary>
    public abstract class ApiClientBase
    {
        // PostAsJsonAsync alapbeállításaival egyező (web: camelCase, kis-nagybetű független).
        private static readonly JsonSerializerOptions RequestJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ILogger _logger;

        protected ApiClientBase(HttpClient http, ILogger logger)
        {
            Http = http;
            _logger = logger;
        }

        /// <summary>Az IHttpClientFactory-től kapott kliens. A fejlécek kérésenként készülnek, ezen soha nem állítunk állapotot.</summary>
        protected HttpClient Http { get; }

        /// <summary>Az utolsó hívás hibaüzenete. Minden hívás elején törlődik.</summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>Az adott kéréshez tartozó Authorization fejléc; null = névtelen hívás.</summary>
        protected virtual Task<AuthenticationHeaderValue?> GetAuthorizationAsync(string uri)
            => Task.FromResult<AuthenticationHeaderValue?>(null);

        /// <summary>A szerver HTTP 2xx-szel, de IsSuccess=false borítékkal válaszolt.</summary>
        protected virtual void OnServerFailure(string message) => ErrorMessage = message;

        protected async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string uri, object? body = null, CancellationToken ct = default)
        {
            ErrorMessage = string.Empty;
            try
            {
                using var request = new HttpRequestMessage(method, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = await GetAuthorizationAsync(uri);
                if (body is not null)
                {
                    request.Content = JsonContent.Create(body, body.GetType(), options: RequestJsonOptions);
                }

                using var response = await Http.SendAsync(request, ct);
                var text = await response.Content.ReadAsStringAsync(ct);
                var root = TryParseJson(text);
                var envelope = AsEnvelope(root);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = BuildHttpError(response, envelope, text);
                    return new ApiResult<T>(false, default);
                }

                if (envelope is { IsSuccess: false })
                {
                    OnServerFailure(envelope.ErrorMessage);
                    return new ApiResult<T>(false, default);
                }

                if (root is null && !string.IsNullOrWhiteSpace(text))
                {
                    if (typeof(T) == typeof(string)) return new ApiResult<T>(true, (T)(object)text);
                    throw new FormatException("A szerver válasza nem értelmezhető JSON-ként.");
                }

                var token = envelope is null ? root : envelope.Content;
                var value = token is null || token.Type == JTokenType.Null ? default : token.ToObject<T>();
                return new ApiResult<T>(true, value);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "A szerver nem válaszolt időben.";
            }
            catch (HttpRequestException e)
            {
                ErrorMessage = "Nem elérhető a szerver! Hibaüzenet: " + e.Message;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "API-hiba: {Method} {Uri}", method, uri);
                ErrorMessage = "Hiba történt! Az üzenet: " + e.Message;
            }
            return new ApiResult<T>(false, default);
        }

        private static string BuildHttpError(HttpResponseMessage response, Envelope? envelope, string text)
        {
            if (envelope is { ErrorMessage.Length: > 0 }) return envelope.ErrorMessage;
            if (response.StatusCode == HttpStatusCode.NotFound) return "Nem elérhető a szerver!";
            var snippet = text.Length > 300 ? text[..300] : text;
            return $"{(int)response.StatusCode} {response.ReasonPhrase}: {snippet}".TrimEnd(' ', ':');
        }

        // DateParseHandling.None: a szöveges mezők (pl. kérdés- vagy válaszszöveg) ne alakuljanak át dátummá.
        private static JToken? TryParseJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            try
            {
                using var reader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.None };
                return JToken.Load(reader);
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        // Boríték = gyökér objektum, amiben MINDKÉT kulcs szerepel (isSuccess, errorMessage).
        // Ez szigorúbb a korábbi `Contains("errorMessage")` szövegkeresésnél, ami bármilyen tartalomra ráillhetett.
        private static Envelope? AsEnvelope(JToken? root)
        {
            if (root is not JObject obj) return null;
            if (!obj.TryGetValue("isSuccess", StringComparison.OrdinalIgnoreCase, out var ok) ||
                !obj.TryGetValue("errorMessage", StringComparison.OrdinalIgnoreCase, out var err))
            {
                return null;
            }
            obj.TryGetValue("content", StringComparison.OrdinalIgnoreCase, out var content);
            return new Envelope((bool?)ok == true, (string?)err ?? string.Empty, content);
        }

        private sealed record Envelope(bool IsSuccess, string ErrorMessage, JToken? Content);
    }

    /// <summary>Egy API-hívás eredménye: sikeres volt-e, és mi a (típusos) értéke.</summary>
    public sealed record ApiResult<T>(bool IsSuccess, T? Value);
}
