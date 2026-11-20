using System.Text.RegularExpressions;

using AngleSharp.Dom;

using Ganss.Xss;

using Microsoft.AspNetCore.Components;

namespace TeszterLibrary.Services.Html
{
    /// <summary>
    /// Tárolt (felhasználó által bevitt) HTML biztonságos megjelenítése. Minden <see cref="MarkupString"/>-es
    /// megjelenítés ezen megy át: a mentési út (EditorHtmlSanitizer) csak az új tartalmat tisztítja, a régi,
    /// adatbázisban lévő és az API-ból érkező HTML-t itt, megjelenítéskor szűrjük (Ganss.Xss engedélyezőlista).
    /// A SafeHtmlUsageTests őrteszt ellenőrzi, hogy a .razor fájlokban nincs közvetlen MarkupString-konverzió.
    /// </summary>
    public static class SafeHtml
    {
        // Csak a szerkesztő által előállított raszteres képformátumok; SVG nem (szkriptet tartalmazhat).
        private static readonly Regex DataImage = new(
            @"^data:image/(png|jpe?g|gif|webp);base64,[A-Za-z0-9+/=\s]+$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        // A beállítás után a Sanitize szálbiztos, ezért egyetlen példányt használunk.
        private static readonly HtmlSanitizer Sanitizer = Create();

        /// <summary>A megtisztított HTML megjelenítésre kész formában. A null üres tartalmat ad.</summary>
        public static MarkupString Markup(string? html) => new(Sanitize(html));

        /// <summary>A megtisztított HTML szövegként.</summary>
        public static string Sanitize(string? html)
            => string.IsNullOrWhiteSpace(html) ? string.Empty : Sanitizer.Sanitize(html);

        private static HtmlSanitizer Create()
        {
            var sanitizer = new HtmlSanitizer();

            // A Quill/TinyMCE szerkesztő által használt osztály- és listajelölők.
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("data-list");

            // Űrlapelemek nem kellenek a kérdésszövegekbe: egy beágyazott űrlap adathalászatra (külső action) használható.
            foreach (var tag in new[] { "form", "input", "button", "select", "option", "optgroup", "textarea", "label", "fieldset", "legend" })
            {
                sanitizer.AllowedTags.Remove(tag);
            }

            // Csak http(s), mailto és relatív hivatkozás; data: kizárólag képként (lásd lent).
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedSchemes.Add("https");
            sanitizer.AllowedSchemes.Add("mailto");

            sanitizer.FilterUrl += (_, e) =>
            {
                if (e.SanitizedUrl is null
                    && e.Tag is { } tag
                    && string.Equals(tag.LocalName, "img", StringComparison.OrdinalIgnoreCase)
                    && DataImage.IsMatch(e.OriginalUrl))
                {
                    e.SanitizedUrl = e.OriginalUrl;   // a régebbi, beágyazott (base64) képek továbbra is látszanak
                }
            };

            // Új lapon nyíló hivatkozás ne férhessen hozzá az alkalmazás ablakához (window.opener).
            sanitizer.PostProcessNode += (_, e) =>
            {
                if (e.Node is IElement { LocalName: "a" } link && link.HasAttribute("target"))
                {
                    link.SetAttribute("rel", "noopener noreferrer");
                }
            };

            return sanitizer;
        }
    }
}
