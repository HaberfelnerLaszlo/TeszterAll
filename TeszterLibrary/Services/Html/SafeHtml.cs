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

        // A matek-feladatok műveletsora (OperationView.Sequence) SVG-ként érkezik az API-ból. Az általános szűrő az
        // SVG-t a tartalmával együtt törli (üres feladat jelent meg), ezért ehhez egy külön, bővebb példány tartozik.
        // Csak rajzoló elemek és megjelenési attribútumok: nincs script, foreignObject, image, animáció, style elem,
        // az on* eseményattribútumok pedig itt sem engedélyezettek. A MathJax a betűjeleket <defs>-ben, id-vel ellátott
        // <path>-ként tárolja és <use xlink:href="#..."/>-val rajzolja ki: ezért a defs/use, az id és a href is
        // megengedett. Az SVG-azonosítók a "tsvg-" előtagot kapják (így nem írhatnak felül oldalelemet/globális nevet),
        // a href csak ugyanilyen előtagú belső (#) hivatkozás lehet; HTML-elemen az id és a nem-belső href nem marad meg.
        private static readonly string[] SvgTags =
        [
            "svg", "g", "defs", "use", "path", "line", "polyline", "polygon", "rect", "circle", "ellipse", "text", "tspan", "title", "desc",
        ];

        private static readonly string[] SvgAttributes =
        [
            "viewbox", "preserveaspectratio", "width", "height", "x", "y", "x1", "y1", "x2", "y2", "cx", "cy", "r", "rx", "ry",
            "dx", "dy", "d", "points", "transform", "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width",
            "stroke-opacity", "stroke-linecap", "stroke-linejoin", "stroke-dasharray", "opacity", "font-size",
            "font-family", "font-weight", "font-style", "text-anchor", "dominant-baseline", "baseline-shift",
            "id", "href", "xlink:href",
        ];

        private const string SvgIdPrefix = "tsvg-";
        private const string SvgNamespace = "http://www.w3.org/2000/svg";

        // Azonosító: betű, szám, '_', '-', '.', ':' (pl. MathJax 3 "MJX-1-TEX-N-31", MathJax 2 "E1-MJMAIN-31").
        private static readonly Regex SvgId = new(@"^[A-Za-z][A-Za-z0-9_.:-]*$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        // A tömbök után: a statikus mezők deklarációs sorrendben inicializálódnak.
        private static readonly HtmlSanitizer SvgSanitizer = CreateWithSvg();

        /// <summary>
        /// Matek-feladatok (SVG-ben tárolt műveletsor) biztonságos megjelenítése: az általános szűrés, kiegészítve
        /// az SVG rajzoló elemeivel. Más tárolt HTML-hez a <see cref="Markup"/>-ot kell használni.
        /// </summary>
        public static MarkupString MarkupWithSvg(string? html) => new(SanitizeWithSvg(html));

        /// <summary>A <see cref="MarkupWithSvg"/> szöveges változata.</summary>
        public static string SanitizeWithSvg(string? html)
            => string.IsNullOrWhiteSpace(html) ? string.Empty : SvgSanitizer.Sanitize(html);

        private static HtmlSanitizer CreateWithSvg()
        {
            var sanitizer = Create();
            foreach (var tag in SvgTags) sanitizer.AllowedTags.Add(tag);
            foreach (var attribute in SvgAttributes) sanitizer.AllowedAttributes.Add(attribute);

            sanitizer.PostProcessNode += (_, e) =>
            {
                if (e.Node is not IElement element)
                {
                    return;
                }
                var isSvg = element.NamespaceUri == SvgNamespace;
                foreach (var attribute in element.Attributes.ToList())
                {
                    var name = attribute.Name;
                    var value = attribute.Value;

                    // a fill/stroke stb. url(...) értéke külső erőforrást tölthetne be
                    if (value.Contains("url(", StringComparison.OrdinalIgnoreCase))
                    {
                        element.RemoveAttribute(name);
                    }
                    else if (name.Equals("id", StringComparison.OrdinalIgnoreCase))
                    {
                        // id csak SVG-ben, előtaggal (nincs DOM-clobbering)
                        if (isSvg && SvgId.IsMatch(value)) element.SetAttribute(name, SvgIdPrefix + value);
                        else element.RemoveAttribute(name);
                    }
                    else if (isSvg && (name.Equals("href", StringComparison.OrdinalIgnoreCase) || name.Equals("xlink:href", StringComparison.OrdinalIgnoreCase)))
                    {
                        // SVG-ben href csak belső hivatkozás lehet, az id-hez hasonlóan előtaggal
                        if (value.StartsWith('#') && SvgId.IsMatch(value[1..])) element.SetAttribute(name, "#" + SvgIdPrefix + value[1..]);
                        else element.RemoveAttribute(name);
                    }
                }
            };
            return sanitizer;
        }

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
