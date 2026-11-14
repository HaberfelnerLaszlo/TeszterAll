using TeszterLibrary.Services.Images;
using TeszterTests.Helpers;

namespace TeszterTests.Services.Images
{
    public class ImageSnifferTests
    {
        [Fact] public void Detects_png() => Assert.Equal(ImageSniffer.Png, ImageSniffer.Detect(TestImages.Png()));
        [Fact] public void Detects_jpeg() => Assert.Equal(ImageSniffer.Jpeg, ImageSniffer.Detect(TestImages.Jpeg()));
        [Fact] public void Detects_gif() => Assert.Equal(ImageSniffer.Gif, ImageSniffer.Detect(TestImages.Gif()));
        [Fact] public void Detects_webp() => Assert.Equal(ImageSniffer.Webp, ImageSniffer.Detect(TestImages.Webp()));

        [Fact]
        public void The_extension_comes_from_the_content_not_from_any_client_supplied_name()
        {
            Assert.Equal("jpg", ImageSniffer.Detect(TestImages.Jpeg())!.Extension);
            Assert.Equal("image/jpeg", ImageSniffer.Detect(TestImages.Jpeg())!.ContentType);
        }

        [Fact]
        public void Svg_html_and_empty_input_are_not_images()
        {
            Assert.Null(ImageSniffer.Detect(TestImages.Svg()));
            Assert.Null(ImageSniffer.Detect("<html><script>alert(1)</script></html>"u8));
            Assert.Null(ImageSniffer.Detect(Array.Empty<byte>()));
            Assert.Null(ImageSniffer.Detect(new byte[] { 0x89, 0x50 }));    // csonka PNG-fejléc
        }

        [Theory]
        [InlineData("image/png", "png", true)]
        [InlineData("IMAGE/PNG", "png", true)]
        [InlineData("image/jpeg", "jpeg", true)]
        [InlineData("image/jpg", "jpeg", true)]
        [InlineData("image/pjpeg", "jpeg", true)]
        [InlineData("image/gif", "gif", true)]
        [InlineData("image/webp", "webp", true)]
        [InlineData("image/png", "jpeg", false)]
        [InlineData("image/svg+xml", "png", false)]
        [InlineData("text/html", "png", false)]
        public void Declared_mime_type_must_match_the_detected_one(string declared, string kind, bool expected)
        {
            var detected = ImageSniffer.Detect(kind switch
            {
                "png" => TestImages.Png(),
                "jpeg" => TestImages.Jpeg(),
                "gif" => TestImages.Gif(),
                _ => TestImages.Webp(),
            })!;

            Assert.Equal(expected, ImageSniffer.DeclaredMimeMatches(declared, detected));
        }
    }
}
