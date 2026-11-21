using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Logging;

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
        private readonly ILogger _logger;

        protected ApiClientBase(HttpClient http, ILogger logger)
        {
            Http = http;
            _logger = logger;
        }

        /// <summary>Az IHttpClientFactory-től kapott kliens. A fejlécek kérésenként készülnek, ezen soha nem állítunk állapotot.</summary>
        protected HttpClient Http { get; }

        /// <summary>Az utolsó hívás HTTP-státusza (null, ha nem jött válasz). Minden hívás elején törlődik.</summary>
        public HttpStatusCode? LastStatusCode { get; private set; }

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
            LastStatusCode = null;
            try
            {
                using var request = new HttpRequestMessage(method, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = await GetAuthorizationAsync(uri);
                if (body is not null)
                {
                    request.Content = JsonContent.Create(body, body.GetType(), options: ApiJson.Request);
                }

                using var response = await Http.SendAsync(request, ct);
                LastStatusCode = response.StatusCode;
                var text = await response.Content.ReadAsStringAsync(ct);
                using var document = TryParseJson(text);
                JsonElement? root = document?.RootElement;
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

                var element = envelope is null ? root : envelope.Content;
                var value = element is not { ValueKind: not JsonValueKind.Null } present ? default : present.Deserialize<T>(ApiJson.Response);
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

        // A System.Text.Json a dátumnak látszó szöveget nem alakítja át (régen ehhez DateParseHandling.None kellett).
        private static JsonDocument? TryParseJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            try
            {
                return JsonDocument.Parse(text, ApiJson.Document);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // Boríték = gyökér objektum, amiben MINDKÉT kulcs szerepel (isSuccess, errorMessage).
        // Ez szigorúbb a korábbi `Contains("errorMessage")` szövegkeresésnél, ami bármilyen tartalomra ráillhetett.
        private static Envelope? AsEnvelope(JsonElement? root)
        {
            if (root is not { ValueKind: JsonValueKind.Object } obj) return null;
            if (Property(obj, "isSuccess") is not { } ok || Property(obj, "errorMessage") is not { } err)
            {
                return null;
            }
            return new Envelope(
                ok.ValueKind == JsonValueKind.True,
                err.ValueKind == JsonValueKind.String ? err.GetString() ?? string.Empty : string.Empty,
                Property(obj, "content"));
        }

        // Kis-nagybetű független kulcskeresés (a JsonElement.TryGetProperty pontos egyezést vár).
        private static JsonElement? Property(JsonElement obj, string name)
        {
            foreach (var property in obj.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)) return property.Value;
            }
            return null;
        }

        private sealed record Envelope(bool IsSuccess, string ErrorMessage, JsonElement? Content);
    }

    /// <summary>Egy API-hívás eredménye: sikeres volt-e, és mi a (típusos) értéke.</summary>
    public sealed record ApiResult<T>(bool IsSuccess, T? Value);
}
