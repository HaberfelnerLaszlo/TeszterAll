using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TeszterLibrary.Services.Images
{
    public static class ImageStorageExtensions
    {
        public static IServiceCollection AddImageStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
            services.AddSingleton<IImageStorage, ImageStorage>();
            return services;
        }

        /// <summary>
        /// GET /uploads/{fájlnév} – csak bejelentkezett felhasználónak. A válasz fejlécei megakadályozzák,
        /// hogy egy kép (vagy képnek álcázott fájl) a böngészőben szkriptet futtasson.
        /// </summary>
        public static IEndpointRouteBuilder MapImageUploads(this IEndpointRouteBuilder endpoints)
        {
            var storage = endpoints.ServiceProvider.GetRequiredService<IImageStorage>();

            endpoints.MapGet(storage.RequestPath + "/{fileName}", IResult (string fileName, HttpContext context, IImageStorage images) =>
            {
                if (!images.TryResolve(fileName, out var path, out var contentType)) return Results.NotFound();

                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["Content-Security-Policy"] = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
                headers["Cache-Control"] = "private, max-age=86400";
                return Results.File(path, contentType);
            }).RequireAuthorization();

            // Figyelmeztetés: a wwwroot/uploads tartalmát a statikus fájlszerver HITELESÍTÉS NÉLKÜL szolgálja ki.
            var env = endpoints.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var legacy = Path.Combine(env.WebRootPath ?? string.Empty, storage.RequestPath.Trim('/'));
            if (Directory.Exists(legacy) && Directory.EnumerateFiles(legacy).Any())
            {
                endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ImageStorage").LogWarning(
                    "A(z) {Legacy} mappa fájljai nyilvánosan, bejelentkezés nélkül elérhetők. Költöztesd őket a tárolási mappába (Uploads:StoragePath, alapértelmezés: App_Data/uploads).",
                    legacy);
            }
            return endpoints;
        }
    }
}
