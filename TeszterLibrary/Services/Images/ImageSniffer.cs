namespace TeszterLibrary.Services.Images
{
    public sealed record ImageType(string Extension, string ContentType);

    /// <summary>
    /// A kép valódi típusát a TARTALOM első bájtjaiból állapítja meg (magic bytes).
    /// A kliens által küldött MIME-típusnak/kiterjesztésnek nincs jelentősége a biztonság szempontjából.
    /// Szándékosan nincs benne SVG: az szkriptet is tartalmazhat.
    /// </summary>
    public static class ImageSniffer
    {
        public static readonly ImageType Png = new("png", "image/png");
        public static readonly ImageType Jpeg = new("jpg", "image/jpeg");
        public static readonly ImageType Gif = new("gif", "image/gif");
        public static readonly ImageType Webp = new("webp", "image/webp");

        private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

        public static ImageType? Detect(ReadOnlySpan<byte> data)
        {
            if (data.Length >= 8 && data[..8].SequenceEqual(PngSignature)) return Png;
            if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF) return Jpeg;
            if (data.Length >= 6 && (data[..6].SequenceEqual("GIF87a"u8) || data[..6].SequenceEqual("GIF89a"u8))) return Gif;
            if (data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data.Slice(8, 4).SequenceEqual("WEBP"u8)) return Webp;
            return null;
        }

        /// <summary>A deklarált MIME-típus (data:image/...) megfelel-e az észlelt típusnak? (jpg/jpeg/pjpeg aliasok kezelve)</summary>
        public static bool DeclaredMimeMatches(string declaredMime, ImageType detected) => declaredMime.ToLowerInvariant() switch
        {
            "image/png" => detected == Png,
            "image/jpeg" or "image/jpg" or "image/pjpeg" => detected == Jpeg,
            "image/gif" => detected == Gif,
            "image/webp" => detected == Webp,
            _ => false,
        };
    }
}
