using System.Text;

namespace TeszterTests.Helpers
{
    /// <summary>Apró, de a típusfelismerés számára érvényes "képek" (csak a fejléc számít).</summary>
    internal static class TestImages
    {
        public static byte[] Png(int extra = 20)
            => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.Concat(Enumerable.Range(0, extra).Select(i => (byte)i)).ToArray();

        public static byte[] Jpeg() => new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4 };
        public static byte[] Gif() => Encoding.ASCII.GetBytes("GIF89a\x01\x00\x01\x00");
        public static byte[] Webp() => Encoding.ASCII.GetBytes("RIFF\x10\x00\x00\x00WEBPVP8 ");
        public static byte[] Svg() => Encoding.UTF8.GetBytes("<svg xmlns='http://www.w3.org/2000/svg' onload='alert(1)'/>");

        public static string DataUri(string mime, byte[] bytes) => $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
