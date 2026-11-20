using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Identity.Client;

namespace TeszterWeb.Services
{
    /// <summary>A bejelentkezéshez tartozó, a Program.cs-ből kiemelt (és így tesztelhető) beállítások.</summary>
    public static class AuthSetup
    {
        /// <summary>
        /// A bejelentkezéskor előre engedélyeztetett (consent) scope-ok: csak a Microsoft Graph-éi.
        /// A TeszterApi tokenjét a TeszterApiService kéri kérésenként (TeszterApi:BaseUrl + Scopes); azt itt
        /// szándékosan nem kérjük, mert egy bejelentkezési kérés csak egy erőforrás scope-jait tartalmazhatja (AADSTS28000).
        /// Régen a kód a TeszterApi:Scopes-t is megpróbálta, de az tömb, így mindig null lett – ez most kimondott döntés.
        /// </summary>
        public static string[]? InitialScopes(IConfiguration configuration)
        {
            var scopes = configuration["MicrosoftGraph:Scopes"]?
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return scopes is { Length: > 0 } ? scopes : null;
        }

        /// <summary>Igaz, ha a kivétel (vagy a belső kivételei) szerint a felhasználónak újra be kell jelentkeznie.</summary>
        public static bool RequiresInteraction(Exception? exception)
        {
            for (var e = exception; e is not null; e = e.InnerException)
            {
                if (e is MsalUiRequiredException)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Ha a tokenszerzéshez interakció kell (pl. lejárt a token-gyorsítótár egy újraindítás után), kijelentkeztetjük
    /// a sütiből, és ugyanarra az oldalra irányítunk, ami új bejelentkezést indít. Minden más kivétel az /Error oldalra megy.
    /// Régen két UseExceptionHandler volt: a belső (/Error) minden kivételt elnyelt, így ez az ág sosem futott le.
    /// </summary>
    public sealed class MsalUiRequiredExceptionHandler(ILogger<MsalUiRequiredExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (!AuthSetup.RequiresInteraction(exception) || httpContext.Response.HasStarted)
            {
                return false;
            }

            logger.LogInformation("Újra kell hitelesíteni a felhasználót: {Path}", httpContext.Request.Path);
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Az UseExceptionHandler ekkorra már /Error-ra írta át a kérés útvonalát; az eredetit a feature őrzi.
            // Csak helyi útvonal + lekérdezés: nem lehet vele külső oldalra irányítani.
            var request = httpContext.Request;
            var originalPath = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path ?? request.Path.Value;
            httpContext.Response.Redirect(UriHelper.BuildRelative(request.PathBase, originalPath, request.QueryString));
            return true;
        }
    }
}
