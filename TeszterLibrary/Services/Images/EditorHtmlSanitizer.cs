using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace TeszterLibrary.Services.Images
{
    /// <summary>
    /// Engedélyezőlistás HTML-tisztító a rich text szerkesztők (Quill) kimenetéhez.
    /// MENTÉSKOR fut, a szerveren, mert a böngészőből érkező HTML nem megbízható (bárki küldhet bármit).
    /// Csak a felsorolt elemek és attribútumok maradnak; minden más törlődik vagy kicsomagolódik.
    /// A többi MarkupString-es megjelenítő helyen továbbra is érdemes érett könyvtárat (Ganss.Xss) használni.
    /// </summary>
    internal sealed class EditorHtmlSanitizer
    {
        public const int MaxDepth = 64;

        // Ezek tartalmával együtt törlődnek.
        private static readonly HashSet<string> DropWithContent = new(StringComparer.OrdinalIgnoreCase)
        {
            "script","style","iframe","frame","frameset","object","embed","applet","svg","math","template","noscript",
            "textarea","title","head","link","meta","base","form","input","button","select","option","audio","video",
            "source","track","canvas","xmp","plaintext","listing","noembed","noframes","portal","dialog","map","area",
        };

        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "p","br","hr","strong","b","em","i","u","s","strike","del","ins","sub","sup","span","div",
            "h1","h2","h3","h4","h5","h6","ul","ol","li","blockquote","pre","code","a","img",
            "table","thead","tbody","tfoot","tr","td","th","caption",
        };

        private static readonly HashSet<string> AllowedStyleProps = new(StringComparer.OrdinalIgnoreCase)
        {
            "color","background-color","font-size","font-weight","font-style","text-decoration","text-align",
            "width","height","max-width","float","display","margin","margin-left","margin-right","margin-top",
            "margin-bottom","vertical-align","line-height",
        };

        private static readonly Regex ClassRegex = new(@"^[A-Za-z0-9_\- ]{1,100}$", RegexOptions.Compiled);
        private static readonly Regex DataListRegex = new(@"^(bullet|ordered|checked|unchecked)$", RegexOptions.Compiled);
        private static readonly Regex SizeRegex = new(@"^\d{1,4}(px|%)?$", RegexOptions.Compiled);
        private static readonly Regex SpanRegex = new(@"^\d{1,2}$", RegexOptions.Compiled);
        private static readonly Regex StyleValueRegex = new(@"^[a-zA-Z0-9\s#%.,()\-+]*$", RegexOptions.Compiled);
        private static readonly Regex UnsafeUrlChars = new("[\\x00-\\x20\"'<>\\\\`]", RegexOptions.Compiled);

        private readonly string _ownImagePrefix;

        public EditorHtmlSanitizer(string requestPath)
        {
            _ownImagePrefix = requestPath.TrimEnd('/') + "/";
        }

        /// <summary>
        /// Megtisztítja a HTML-t. A még feltöltésre váró data:image képeket a <paramref name="pendingImages"/> listába gyűjti
        /// (az src-jük egyelőre megmarad). Null = elutasítva (túl mély beágyazás).
        /// </summary>
        public HtmlDocument? Sanitize(string html, List<string> warnings, out List<HtmlNode> pendingImages)
        {
            pendingImages = new List<HtmlNode>();
            var doc = new HtmlDocument
            {
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
                OptionMaxNestedChildNodes = MaxDepth + 8,
            };
            try
            {
                doc.LoadHtml(html);
            }
            catch (Exception)   // pl. a beágyazási mélység túllépése
            {
                warnings.Add("A tartalom szerkezete érvénytelen (túl mély beágyazás).");
                return null;
            }

            var stack = new Stack<(HtmlNode Node, int Depth)>();
            foreach (var c in doc.DocumentNode.ChildNodes.ToList()) stack.Push((c, 1));

            while (stack.Count > 0)
            {
                var (node, depth) = stack.Pop();
                if (depth > MaxDepth)
                {
                    warnings.Add("A tartalom túl mélyen van egymásba ágyazva.");
                    return null;
                }

                switch (node.NodeType)
                {
                    case HtmlNodeType.Comment:
                        node.Remove();
                        break;

                    case HtmlNodeType.Text:
                        // Nyers < és > soha nem maradhat szövegben (a böngésző másképp értelmezhetné, mint a HAP).
                        var t = (HtmlTextNode)node;
                        t.Text = t.Text.Replace("<", "&lt;").Replace(">", "&gt;");
                        break;

                    case HtmlNodeType.Element:
                        var name = node.Name.ToLowerInvariant();
                        if (DropWithContent.Contains(name))
                        {
                            node.Remove();
                            break;
                        }
                        if (!AllowedTags.Contains(name))
                        {
                            var kids = node.ChildNodes.ToList();
                            node.ParentNode.RemoveChild(node, true);   // kicsomagolás: a gyerekek a helyére kerülnek
                            foreach (var k in kids) stack.Push((k, depth));
                            break;
                        }
                        CleanAttributes(node, name, warnings, pendingImages, out var keep);
                        if (!keep)
                        {
                            node.Remove();
                            break;
                        }
                        foreach (var k in node.ChildNodes.ToList()) stack.Push((k, depth + 1));
                        break;

                    default:
                        node.Remove();
                        break;
                }
            }
            return doc;
        }

        private void CleanAttributes(HtmlNode node, string tag, List<string> warnings, List<HtmlNode> pendingImages, out bool keep)
        {
            keep = true;
            foreach (var attr in node.Attributes.ToList())
            {
                var name = attr.Name.ToLowerInvariant();
                var value = HtmlEntity.DeEntitize(attr.Value ?? string.Empty);   // először dekódolunk, azt ellenőrizzük
                string? clean = null;

                switch (name)
                {
                    case "class": clean = ClassRegex.IsMatch(value) ? value : null; break;
                    case "style": clean = CleanStyle(value); break;
                    case "data-list": clean = DataListRegex.IsMatch(value) ? value : null; break;
                    case "title" when tag is "a" or "img": clean = value.Length <= 300 ? value : null; break;
                    case "href" when tag == "a": clean = CleanUrl(value, allowMailto: true, allowOwnRelative: true); break;
                    case "target" when tag == "a": clean = value == "_blank" ? value : null; break;
                    case "alt" when tag == "img": clean = value.Length <= 300 ? value : string.Empty; break;
                    case "width" or "height" when tag == "img": clean = SizeRegex.IsMatch(value) ? value : null; break;
                    case "colspan" or "rowspan" when tag is "td" or "th": clean = SpanRegex.IsMatch(value) ? value : null; break;
                    case "src" when tag == "img":
                        if (value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                        {
                            pendingImages.Add(node);          // feltöltés később; addig érintetlen marad
                            continue;
                        }
                        clean = CleanUrl(value, allowMailto: false, allowOwnRelative: true);
                        if (clean is null)
                        {
                            warnings.Add("Egy kép elavult vagy nem engedélyezett forrásról származik, ezért eltávolítottuk.");
                            keep = false;
                            return;
                        }
                        break;
                }

                if (clean is null)
                {
                    attr.Remove();
                }
                else
                {
                    attr.Value = EncodeAttribute(clean);          // kimenetnél mindig kódolunk
                    attr.QuoteType = AttributeValueQuote.DoubleQuote;
                }
            }

            if (tag == "img" && !node.Attributes.Contains("src"))
            {
                keep = false;
                return;
            }
            if (tag == "a" && node.Attributes.Contains("target"))
            {
                node.SetAttributeValue("rel", "noopener noreferrer");
            }
        }

        /// <summary>
        /// Attribútumérték kódolása. Saját, mert a HtmlAgilityPack Entitize()-ja nem kódolja a &lt; &gt; " ' karaktereket
        /// és az &amp;-t sem (ez attribútum-kitöréshez, illetve a "&amp;copy=" típusú URL-paraméterek hibás értelmezéséhez vezetne).
        /// </summary>
        internal static string EncodeAttribute(string value)
        {
            var sb = new System.Text.StringBuilder(value.Length + 8);
            foreach (var c in value)
            {
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '\'': sb.Append("&#39;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '`': sb.Append("&#96;"); break;
                    default: if (c >= ' ' || c == '\t') sb.Append(c); break;   // vezérlőkarakterek kiszűrve
                }
            }
            return sb.ToString();
        }

        private string? CleanUrl(string value, bool allowMailto, bool allowOwnRelative)
        {
            value = value.Trim();
            if (value.Length is 0 or > 2000 || UnsafeUrlChars.IsMatch(value)) return null;

            if (allowOwnRelative && value.StartsWith(_ownImagePrefix, StringComparison.Ordinal) && !value.Contains(".."))
                return value;

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme is "http" or "https") return value;
                if (allowMailto && uri.Scheme == "mailto") return value;
            }
            return null;
        }

        private static string? CleanStyle(string value)
        {
            var kept = new List<string>();
            foreach (var decl in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var i = decl.IndexOf(':');
                if (i <= 0) continue;
                var prop = decl[..i].Trim();
                var val = decl[(i + 1)..].Trim();
                if (!AllowedStyleProps.Contains(prop) || !StyleValueRegex.IsMatch(val)) continue;
                var lower = val.ToLowerInvariant();
                if (lower.Contains("url") || lower.Contains("expression") || lower.Contains("javascript") || lower.Contains("var(")) continue;
                kept.Add($"{prop.ToLowerInvariant()}: {val}");
            }
            return kept.Count == 0 ? null : string.Join("; ", kept);
        }
    }
}
