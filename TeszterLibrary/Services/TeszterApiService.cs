using System.Net.Http.Headers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;

using TeszterLibrary.Events;

namespace TeszterLibrary.Services
{
    /// <summary>
    /// A Teszter API kliense. A felhasználói tokent KÉRÉSENKÉNT kéri le (az MSAL gyorsítótárból,
    /// lejárat előtt frissítve) és a kérésre teszi, nem a megosztott HttpClient-re, így
    /// a felhasználók tokenjei nem keveredhetnek.
    /// </summary>
    public class TeszterApiService : ApiClientBase
    {
        public const string HttpClientName = "TeszterApi";

        // Token nélkül csak a kivetítős órai nézet (ClassLessonHost) három végpontja megy; az API-n is csak ezek névtelenek
        // (TeszterData: MathController [AllowAnonymous]). Régen minden "Math..." hívás token nélkül ment, így a diák- és
        // tanári Math-végpontok (eredmények, órák mentése) csak névtelen API mellett működhettek.
        private static readonly string[] AnonymousExact = ["Math/Groups", "Math/ClassResult"];
        private const string AnonymousClassPrefix = "Math/Class/";   // GET Math/Class/{id}; NEM a Math/Class (POST) és a Math/ClassLessons/...

        private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;
        private readonly string[] _scopes;

        public EventHandler<MessageEvent>? OnMessageReceived { get; set; }

        public TeszterApiService(
            IHttpClientFactory httpClientFactory,
            IAuthorizationHeaderProvider authorizationHeaderProvider,
            IConfiguration configuration,
            ILogger<TeszterApiService> logger)
            : base(httpClientFactory.CreateClient(HttpClientName), logger)
        {
            _authorizationHeaderProvider = authorizationHeaderProvider
                ?? throw new ArgumentNullException(nameof(authorizationHeaderProvider));
            _scopes = ResolveScopes(configuration);
        }

        public async Task<Tv?> Get<Tv>(string uri)
            => (await SendAsync<Tv>(HttpMethod.Get, uri)).Value;

        public async Task<T?> Post<T>(string uri, T t)
            => (await SendAsync<T>(HttpMethod.Post, uri, t)).Value;

        public async Task<T?> Put<T>(string uri, T t)
            => (await SendAsync<T>(HttpMethod.Put, uri, t)).Value;

        public async Task<bool> Delete(string uri)
            => (await SendAsync<object>(HttpMethod.Delete, uri)).IsSuccess;

        protected override async Task<AuthenticationHeaderValue?> GetAuthorizationAsync(string uri)
        {
            if (IsAnonymous(uri))
            {
                return null;
            }
            var header = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(_scopes);
            return AuthenticationHeaderValue.Parse(header);   // "Bearer <token>"
        }

        /// <summary>Token nélkül hívható-e az útvonal (a kivetítős órai nézet végpontjai).</summary>
        public static bool IsAnonymous(string uri)
        {
            var path = uri.Split('?', 2)[0].TrimStart('/');
            return AnonymousExact.Any(a => string.Equals(path, a, StringComparison.OrdinalIgnoreCase))
                || (path.StartsWith(AnonymousClassPrefix, StringComparison.OrdinalIgnoreCase) && path.Length > AnonymousClassPrefix.Length);
        }

        protected override void OnServerFailure(string message)
            => OnMessageReceived?.Invoke(this, new MessageEvent(message, 100));

        // Scope = TeszterApi:BaseUrl + "/" + TeszterApi:Scopes[i]  (pl. api://.../access_as_user)
        private static string[] ResolveScopes(IConfiguration configuration)
        {
            var baseUrl = configuration["TeszterApi:BaseUrl"]?.TrimEnd('/');
            var scopes = configuration.GetSection("TeszterApi:Scopes").Get<string[]>();
            if (string.IsNullOrEmpty(baseUrl) || scopes is not { Length: > 0 })
            {
                throw new InvalidOperationException("Hiányzik a TeszterApi:BaseUrl vagy a TeszterApi:Scopes beállítás.");
            }
            return scopes.Select(s => s.Contains("://") ? s : $"{baseUrl}/{s}").ToArray();
        }
    }
}
