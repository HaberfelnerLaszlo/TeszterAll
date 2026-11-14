using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TeszterLibrary.Services;
using TeszterLibrary.Services.Feladat;

namespace TeszterLibrary.Extensions
{
    public static class ApiClientServiceCollectionExtensions
    {
        /// <summary>
        /// Regisztrálja a két API-klienst és a hozzájuk tartozó névvel ellátott HttpClient-eket
        /// (IHttpClientFactory: poolozott kapcsolatok, DNS-frissítés, kérésenkénti fejlécek).
        /// Konfig: ApiUrl, FeladatApi:BaseUrl (opc.), FeladatApi:UploadUrl (opc.).
        /// </summary>
        public static IServiceCollection AddTeszterApiClients(this IServiceCollection services)
        {
            services.AddHttpClient(TeszterApiService.HttpClientName, (sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = ToBaseUri(cfg["ApiUrl"] ?? "https://teszter-api.haberfelner.hu/");
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            services.AddHttpClient(FeladatApiService.HttpClientName, (sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = ToBaseUri(cfg["FeladatApi:BaseUrl"] ?? "https://fapi.haberfelner.hu/");
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            // Scoped = egy példány / Blazor circuit (felhasználó). Singletonként a ErrorMessage és a token közös lenne.
            services.AddScoped<TeszterApiService>();
            services.AddScoped<FeladatApiService>();
            return services;
        }

        private static Uri ToBaseUri(string url) => new(url.EndsWith('/') ? url : url + "/");
    }
}
