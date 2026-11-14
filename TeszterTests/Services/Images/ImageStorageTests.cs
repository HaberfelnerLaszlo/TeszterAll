using System.Diagnostics;
using System.Text.RegularExpressions;

using TeszterLibrary.Services.Images;
using TeszterTests.Helpers;

namespace TeszterTests.Services.Images
{
    public class ImageStorageTests : IDisposable
    {
        private readonly TestStorage _test = new();
        private ImageStorage Storage => _test.Storage;

        public void Dispose() => _test.Dispose();

        // ---------------------------------------------------------------- data URI feldolgozás

        [Theory]
        [InlineData("image/png", "png", "png")]
        [InlineData("image/jpeg", "jpeg", "jpg")]
        [InlineData("image/gif", "gif", "gif")]
        [InlineData("image/webp", "webp", "webp")]
        public void Valid_images_are_decoded_and_typed_by_their_content(string mime, string kind, string extension)
        {
            var bytes = kind switch { "png" => TestImages.Png(), "jpeg" => TestImages.Jpeg(), "gif" => TestImages.Gif(), _ => TestImages.Webp() };

            var ok = ImageStorage.TryDecodeDataUri(TestImages.DataUri(mime, bytes), 1000, out var decoded, out var type, out _);

            Assert.True(ok);
            Assert.Equal(extension, type!.Extension);
            Assert.Equal(bytes, decoded);
        }

        [Theory]
        [InlineData("svg-declared")]
        [InlineData("svg-disguised-as-png")]
        [InlineData("jpeg-declared-as-png")]
        [InlineData("path-traversal-type")]
        [InlineData("path-traversal-type-2")]
        [InlineData("html")]
        [InlineData("bad-base64")]
        [InlineData("no-base64-flag")]
        [InlineData("not-a-data-uri")]
        public void Dangerous_or_malformed_data_uris_are_rejected(string kind)
        {
            var uri = kind switch
            {
                "svg-declared" => TestImages.DataUri("image/svg+xml", TestImages.Svg()),
                "svg-disguised-as-png" => TestImages.DataUri("image/png", TestImages.Svg()),
                "jpeg-declared-as-png" => TestImages.DataUri("image/png", TestImages.Jpeg()),
                "path-traversal-type" => "data:image/x/../../../../etc/passwd;base64,AAAA",
                "path-traversal-type-2" => "data:image/png/../../x;base64,AAAA",
                "html" => TestImages.DataUri("text/html", System.Text.Encoding.UTF8.GetBytes("<script>alert(1)</script>")),
                "bad-base64" => "data:image/png;base64,!!!nem-base64!!!",
                "no-base64-flag" => "data:image/png,AAAA",
                _ => "https://example.com/a.png",
            };

            var ok = ImageStorage.TryDecodeDataUri(uri, 1000, out _, out var type, out var error);

            Assert.False(ok);
            Assert.Null(type);
            Assert.NotEmpty(error);
        }

        [Fact]
        public void The_size_limit_is_exact()
        {
            Assert.True(ImageStorage.TryDecodeDataUri(TestImages.DataUri("image/png", TestImages.Png(92)), 100, out _, out _, out _));     // 100 bájt
            Assert.False(ImageStorage.TryDecodeDataUri(TestImages.DataUri("image/png", TestImages.Png(93)), 100, out _, out _, out var error));  // 101 bájt
            Assert.Contains("túl nagy", error);
        }

        [Fact]
        public void A_huge_payload_is_rejected_before_it_is_decoded()
        {
            var huge = "data:image/png;base64," + new string('A', 50_000_000);
            var sw = Stopwatch.StartNew();

            var ok = ImageStorage.TryDecodeDataUri(huge, 2 * 1024 * 1024, out _, out _, out _);

            Assert.False(ok);
            Assert.InRange(sw.ElapsedMilliseconds, 0, 1500);   // a dekódolás ennél jóval tovább tartana
        }

        [Fact]
        public void Line_breaks_and_spaces_inside_the_base64_are_tolerated()
        {
            var b64 = Convert.ToBase64String(TestImages.Png(300)).Insert(40, "\n  \r\n");

            Assert.True(ImageStorage.TryDecodeDataUri("data:image/png;base64," + b64, 1000, out _, out _, out _));
        }

        // ---------------------------------------------------------------- mentés és kiszolgálás

        [Fact]
        public async Task Saving_uses_a_server_generated_name_and_stores_the_exact_bytes()
        {
            var result = await Storage.SaveDataUriAsync(TestImages.DataUri("image/png", TestImages.Png()));

            Assert.True(result.IsSuccess);
            Assert.StartsWith("/uploads/", result.Url);
            var name = Path.GetFileName(result.Url!);
            Assert.Matches(@"^[0-9a-f]{32}\.png$", name);
            Assert.Equal(TestImages.Png(), File.ReadAllBytes(Path.Combine(_test.Root, name)));
            Assert.True(Storage.TryResolve(name, out var path, out var contentType));
            Assert.Equal("image/png", contentType);
            Assert.StartsWith(_test.Root, path);
        }

        [Fact]
        public async Task A_rejected_image_leaves_no_file_behind()
        {
            var result = await Storage.SaveDataUriAsync(TestImages.DataUri("image/png", TestImages.Svg()));

            Assert.False(result.IsSuccess);
            Assert.Equal(0, _test.FileCount);
        }

        [Fact]
        public async Task Two_hundred_parallel_saves_produce_two_hundred_distinct_files()
        {
            var results = await Task.WhenAll(Enumerable.Range(0, 200).Select(_ => Storage.SaveDataUriAsync(TestImages.DataUri("image/png", TestImages.Png()))));

            Assert.All(results, r => Assert.True(r.IsSuccess));
            Assert.Equal(200, results.Select(r => r.Url).Distinct().Count());
            Assert.Equal(200, _test.FileCount);
        }

        [Fact]
        public void Files_written_by_the_old_implementation_can_still_be_served()
        {
            Directory.CreateDirectory(_test.Root);
            File.WriteAllBytes(Path.Combine(_test.Root, "166e795a-e69f-4cd2-a181-25abb0739308.png"), TestImages.Png());
            File.WriteAllBytes(Path.Combine(_test.Root, "166e795a-e69f-4cd2-a181-25abb0739309.jpeg"), TestImages.Jpeg());

            Assert.True(Storage.TryResolve("166e795a-e69f-4cd2-a181-25abb0739308.png", out _, out _));
            Assert.True(Storage.TryResolve("166e795a-e69f-4cd2-a181-25abb0739309.jpeg", out _, out var contentType));
            Assert.Equal("image/jpeg", contentType);
        }

        [Theory]
        [InlineData("../x.png")]
        [InlineData("..%2fx.png")]
        [InlineData("..\\x.png")]
        [InlineData("a.png")]
        [InlineData("/etc/passwd")]
        [InlineData("")]
        [InlineData("166e795a-e69f-4cd2-a181-25abb0739310.svg")]
        [InlineData("166e795a-e69f-4cd2-a181-25abb0739310.exe")]
        [InlineData("166e795a-e69f-4cd2-a181-25abb0739399.png")]
        [InlineData("notaguid.png")]          // létező, de nem a szerver által generált név
        public void Only_well_formed_existing_image_names_are_resolved(string name)
        {
            Directory.CreateDirectory(_test.Root);
            File.WriteAllBytes(Path.Combine(_test.Root, "notaguid.png"), TestImages.Png());
            File.WriteAllText(Path.Combine(_test.Root, "166e795a-e69f-4cd2-a181-25abb0739310.svg"), "<svg/>");
            File.WriteAllText(Path.Combine(_test.Root, "166e795a-e69f-4cd2-a181-25abb0739310.exe"), "MZ");

            Assert.False(Storage.TryResolve(name, out _, out _));
        }
    }
}
