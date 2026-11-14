using TeszterLibrary.Services.Images;
using TeszterTests.Helpers;

namespace TeszterTests.Services.Images
{
    /// <summary>
    /// A szerkesztő HTML-jének mentés előtti feldolgozása: engedélyezőlistás tisztítás + a beágyazott képek fájlba mentése.
    /// </summary>
    public class EditorHtmlProcessingTests : IDisposable
    {
        private static readonly string[] DangerMarkers =
            { "onerror", "onload", "onclick", "onmouse", "<script", "javascript:", "<iframe", "<svg", "<object", "<embed", "expression(", "<style", "<form" };

        private readonly TestStorage _test = new();
        private ImageStorage Storage => _test.Storage;

        public void Dispose() => _test.Dispose();

        private Task<SanitizedHtml> Process(string html) => Storage.ProcessEditorHtmlAsync(html);

        // ---------------------------------------------------------------- támadó bemenetek

        [Theory]
        [InlineData("<img src=x onerror=alert(1)>")]   // img onerror, idézőjel nélkül
        [InlineData("<img src=\"https://e.com/a.png\" onerror=\"alert(1)\" onload=\"x()\">")]   // img onerror/onload https képen
        [InlineData("<script>alert(1)</script><p>ok</p>")]   // script
        [InlineData("<a href=\"javascript:alert(1)\">x</a>")]   // javascript: href
        [InlineData("<a href=\"jav&#x61;script:alert(1)\">x</a>")]   // javascript: entitással
        [InlineData("<a href=\"  javascript:alert(1)\">x</a>")]   // javascript: szóközzel
        [InlineData("<a href=\"java\tscript:alert(1)\">x</a>")]   // javascript: tabbal
        [InlineData("<a href=\"data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==\">x</a>")]   // data: href
        [InlineData("<p onclick=\"x()\" style=\"color:red;position:fixed;background:url(javascript:alert(1))\">t</p>")]   // onclick + veszélyes style
        [InlineData("<svg onload=alert(1)><circle/></svg>")]   // svg
        [InlineData("<iframe src=\"https://evil.example\"></iframe>")]   // iframe
        [InlineData("<object data=x></object><embed src=x><form action=x><input></form>")]   // object/embed/form
        [InlineData("<style>img{background:url(javascript:alert(1))}</style>")]   // style tag
        [InlineData("<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">")]   // noscript mXSS
        [InlineData("<math><mtext><table><mglyph><style><!--</style><img title=\"--&gt;&lt;img src=1 onerror=alert(1)&gt;\">")]   // math mXSS
        [InlineData("<textarea></textarea><img src=x onerror=1>")]   // textarea
        [InlineData("<img alt='\"><script>alert(1)</script>' src=\"https://e.com/a.png\">")]   // alt attribútum kitörés
        [InlineData("<p style=\"width: expression(alert(1)); color: red\">t</p>")]   // css expression
        [InlineData("<img src=\"data:image/svg+xml;base64,PHN2ZyBvbmxvYWQ9YWxlcnQoMSk+\">")]   // data:svg kép
        [InlineData("<img src=\"//evil.com/x.png\" onerror=1>")]   // protokoll-relatív kép
        [InlineData("<img src=\"/etc/passwd\">")]   // fájlútvonal kép
        [InlineData("<p class=\"a\" onmouseover=\"alert(1)\" data-x=\"1\">t</p>")]   // idegen attribútumok
        [InlineData("<p>1 < 2 és <img src=x onerror=alert(1) ></p>")]   // nyers < a szövegben
        public async Task Known_xss_vectors_are_neutralised(string html)
        {
            var result = await Process(html);

            var output = result.Html.ToLowerInvariant();
            Assert.DoesNotContain(DangerMarkers, marker => output.Contains(marker));
        }

        [Fact]
        public async Task Script_is_removed_with_its_content_but_the_surrounding_text_stays()
        {
            var result = await Process("<p>ok</p><script>alert(1)</script>");

            Assert.Contains("ok", result.Html);
            Assert.DoesNotContain("script", result.Html);
            Assert.DoesNotContain("alert", result.Html);       // a script TARTALMA is törlődik, nem marad látható szövegként
        }

        [Fact]
        public async Task An_alt_attribute_cannot_break_out_of_its_quotes()
        {
            var result = await Process("<img alt=\"Tom's &quot;x&quot; <b>\" src=\"https://e.com/a.png\">");

            Assert.DoesNotContain("<b>", result.Html);
            Assert.Contains("&#39;", result.Html);
            Assert.Contains("&quot;", result.Html);
        }

        [Fact]
        public async Task An_ampersand_in_a_link_is_encoded_so_that_copy_is_not_read_as_a_character_reference()
        {
            var result = await Process("<a href=\"https://e.com/?a=1&copy=2\">l</a>");

            Assert.Contains("a=1&amp;copy=2", result.Html);
        }

        [Fact]
        public async Task Raw_angle_brackets_never_remain_in_text()
        {
            var result = await Process("<p>1 < 2 and 3 > 2</p>");

            Assert.Contains("&lt;", result.Html);
            Assert.Equal("<p>", result.Html[..3]);
            Assert.DoesNotContain("<", result.Html[3..].Replace("</p>", ""));
        }

        // ---------------------------------------------------------------- ami megmarad

        [Fact]
        public async Task Basic_formatting_survives()
        {
            var result = await Process("<p>Szia <strong>világ</strong> <em>é</em> <u>u</u></p>");

            Assert.False(result.IsRejected);
            Assert.Contains("<strong>világ</strong>", result.Html);
        }

        [Fact]
        public async Task An_https_link_stays_and_gets_rel_noopener()
        {
            var result = await Process("<a href=\"https://example.com\" target=\"_blank\">l</a>");

            Assert.Contains("href=\"https://example.com\"", result.Html);
            Assert.Contains("rel=\"noopener noreferrer\"", result.Html);
        }

        [Fact]
        public async Task Style_is_filtered_through_a_property_allowlist()
        {
            var result = await Process("<p style=\"color:red;position:fixed;float:left\">t</p>");

            Assert.Contains("color: red", result.Html);
            Assert.Contains("float: left", result.Html);
            Assert.DoesNotContain("position", result.Html);
        }

        [Fact]
        public async Task An_external_https_image_keeps_src_and_size_but_loses_event_handlers()
        {
            var result = await Process("<img src=\"https://example.com/a.png\" width=\"120\" height=\"50px\" alt=\"kép\" style=\"float:left\" onerror=\"x\">");

            Assert.Contains("src=\"https://example.com/a.png\"", result.Html);
            Assert.Contains("width=\"120\"", result.Html);
            Assert.DoesNotContain("onerror", result.Html);
        }

        [Fact]
        public async Task Quill_list_markers_survive()
        {
            var result = await Process("<ol><li data-list=\"bullet\">a</li></ol>");

            Assert.Contains("data-list=\"bullet\"", result.Html);
        }

        [Fact]
        public async Task Unknown_harmless_elements_are_unwrapped_and_comments_removed()
        {
            Assert.Equal("x", (await Process("<font color=\"red\">x</font>")).Html);
            Assert.DoesNotContain("megjegyz", (await Process("<p>a</p><!-- megjegyzés --><p>b</p>")).Html);
        }

        [Theory]
        [InlineData("<p>Szia <strong>x</strong></p>")]
        [InlineData("<img src=\"https://example.com/a.png\" width=\"120\" alt=\"a &amp; b\">")]
        [InlineData("<a href=\"https://e.com/?a=1&b=2\" target=\"_blank\">l</a>")]
        [InlineData("<p>1 &lt; 2</p>")]
        public async Task Processing_the_output_again_changes_nothing(string html)
        {
            var once = (await Process(html)).Html;
            var twice = (await Process(once)).Html;

            Assert.Equal(once, twice);
        }

        // ---------------------------------------------------------------- korlátok

        [Fact]
        public async Task Excessive_nesting_is_rejected_but_reasonable_nesting_is_fine()
        {
            Assert.True((await Process(string.Concat(Enumerable.Repeat("<div>", 100)) + "x")).IsRejected);
            Assert.False((await Process(string.Concat(Enumerable.Repeat("<div>", 40)) + "x" + string.Concat(Enumerable.Repeat("</div>", 40)))).IsRejected);
        }

        [Fact]
        public async Task More_than_twenty_new_images_in_one_save_are_rejected()
        {
            var image = "<img src=\"" + TestImages.DataUri("image/png", TestImages.Png()) + "\">";

            var result = await Process(string.Concat(Enumerable.Repeat(image, 21)));

            Assert.True(result.IsRejected);
            Assert.Equal(0, _test.FileCount);
        }

        [Fact]
        public async Task Oversized_html_is_rejected()
        {
            Assert.True((await Process(new string('a', 4_000_001))).IsRejected);
        }

        // ---------------------------------------------------------------- képek

        [Fact]
        public async Task An_embedded_png_becomes_a_stored_file_with_an_uploads_url()
        {
            var result = await Process($"<p>x</p><img src=\"{TestImages.DataUri("image/png", TestImages.Png())}\" width=\"50\">");

            Assert.DoesNotContain("base64", result.Html);
            Assert.Contains("width=\"50\"", result.Html);
            Assert.Matches(@"src=""/uploads/[0-9a-f]{32}\.png""", result.Html);
            Assert.Equal(1, _test.FileCount);
        }

        [Fact]
        public async Task A_rejected_image_is_removed_with_a_warning_instead_of_staying_as_base64()
        {
            var result = await Process($"<img src=\"{TestImages.DataUri("image/svg+xml", TestImages.Svg())}\"><p>marad</p>");

            Assert.DoesNotContain("img", result.Html);
            Assert.Contains("marad", result.Html);
            Assert.Single(result.Warnings);
            Assert.Equal(0, _test.FileCount);
        }

        [Fact]
        public async Task An_svg_disguised_as_png_is_not_stored()
        {
            var result = await Process($"<img src=\"{TestImages.DataUri("image/png", TestImages.Svg())}\">");

            Assert.DoesNotContain("<img", result.Html);
            Assert.Contains(result.Warnings, w => w.Contains("nem valódi kép"));
            Assert.Equal(0, _test.FileCount);
        }
    }
}
