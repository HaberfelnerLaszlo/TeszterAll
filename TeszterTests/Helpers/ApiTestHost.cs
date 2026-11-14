using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TeszterLibrary.Extensions;
using TeszterLibrary.Services;
using TeszterLibrary.Services.Feladat;

namespace TeszterTests.Helpers
{
    /// <summary>A két API-kliens a valódi DI-regisztrációval (AddTeszterApiClients), de hamis HTTP-kezelővel és tokennel.</summary>
    public sealed class ApiTestHost : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly IServiceScope _scope;

        private TeszterApiService? _api;
        private FeladatApiService? _feladat;

        public StubHttpHandler Handler { get; } = new();

        // Lustán oldjuk fel: hiányos konfigurációnál a hiba az Api elérésekor jelentkezik, nem a konstruktorban.
        public TeszterApiService Api => _api ??= _scope.ServiceProvider.GetRequiredService<TeszterApiService>();
        public FeladatApiService Feladat => _feladat ??= _scope.ServiceProvider.GetRequiredService<FeladatApiService>();

        public ApiTestHost(string token = "tok", Dictionary<string, string?>? config = null)
        {
            var values = new Dictionary<string, string?>
            {
                ["ApiUrl"] = "https://api.test",
                ["TeszterApi:BaseUrl"] = "api://abc",
                ["TeszterApi:Scopes:0"] = "access_as_user",
            };
            if (config is not null) foreach (var kv in config) values[kv.Key] = kv.Value;

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
            services.AddTeszterApiClients();
            services.AddHttpClient(TeszterApiService.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => Handler);
            services.AddHttpClient(FeladatApiService.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => Handler);
            services.AddScoped(_ => FakeAuthorizationHeaderProvider.Create(token));

            _provider = services.BuildServiceProvider();
            _scope = _provider.CreateScope();
        }

        /// <summary>Újabb TeszterApiService-példány egy MÁSIK felhasználó tokenjével (ugyanazon a HttpClient-gyáron).</summary>
        public TeszterApiService CreateApiFor(string token)
            => ActivatorUtilities.CreateInstance<TeszterApiService>(_scope.ServiceProvider, FakeAuthorizationHeaderProvider.Create(token));

        public void Dispose()
        {
            _scope.Dispose();
            _provider.Dispose();
        }
    }
}
