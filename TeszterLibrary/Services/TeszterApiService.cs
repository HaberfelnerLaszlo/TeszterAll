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

        // Ezek az útvonalak eddig is token nélkül mentek (uri.Contains("Math")).
        // Ha a szerver 401-et ad rájuk, ürítsd ezt a listát: akkor a Math API is tokent igényel.
        private static readonly string[] AnonymousPrefixes = ["Math"];

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
            if (AnonymousPrefixes.Any(p => uri.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }
            var header = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(_scopes);
            return AuthenticationHeaderValue.Parse(header);   // "Bearer <token>"
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
