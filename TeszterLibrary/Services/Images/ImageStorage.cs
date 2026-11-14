using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TeszterLibrary.Services.Images
{
    public sealed record ImageSaveResult(bool IsSuccess, string? Url, string? Error);

    /// <param name="Html">A megtisztított HTML (üres, ha el lett utasítva).</param>
    /// <param name="Warnings">Felhasználónak mutatható üzenetek (pl. elutasított képek).</param>
    /// <param name="IsRejected">Igaz: a tartalmat nem szabad menteni.</param>
    public sealed record SanitizedHtml(string Html, IReadOnlyList<string> Warnings, bool IsRejected);

    public interface IImageStorage
    {
        string RequestPath { get; }

        /// <summary>Egy data:image/...;base64,... URI ellenőrzése és mentése. A fájlnevet a szerver generálja.</summary>
        Task<ImageSaveResult> SaveDataUriAsync(string dataUri, CancellationToken ct = default);

        /// <summary>
        /// A szerkesztő HTML-jét megtisztítja (engedélyezőlista), a beágyazott data:image képeket
        /// ellenőrzi és fájlba menti, az src-t a hitelesített /uploads/... címre cseréli.
        /// </summary>
        Task<SanitizedHtml> ProcessEditorHtmlAsync(string html, CancellationToken ct = default);

        /// <summary>A kiszolgáláshoz: csak szabályos fájlnév, csak a tárolási mappán belül.</summary>
        bool TryResolve(string fileName, out string physicalPath, out string contentType);
    }

    public sealed class ImageStorage : IImageStorage
    {
        private const int MaxImagesPerSave = 20;

        // Új fájlok: 32 hexa + kiterjesztés. A régiek (GUID kötőjelekkel, .png/.jpeg/...) is szabályosak.
        private static readonly Regex FileNameRegex = new(
            @"^[0-9a-fA-F]{8}(-?[0-9a-fA-F]{4}){3}-?[0-9a-fA-F]{12}\.(png|jpe?g|gif|webp)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly ImageStorageOptions _options;
        private readonly ILogger<ImageStorage> _logger;
        private readonly EditorHtmlSanitizer _sanitizer;
        private readonly string _root;

        public ImageStorage(IOptions<ImageStorageOptions> options, IWebHostEnvironment environment, ILogger<ImageStorage> logger)
        {
            _options = options.Value;
            _logger = logger;
            RequestPath = "/" + _options.RequestPath.Trim('/');
            _sanitizer = new EditorHtmlSanitizer(RequestPath);
            _root = Path.GetFullPath(string.IsNullOrWhiteSpace(_options.StoragePath)
                ? Path.Combine(environment.ContentRootPath, "App_Data", "uploads")
                : _options.StoragePath);
        }

        public string RequestPath { get; }

        public async Task<ImageSaveResult> SaveDataUriAsync(string dataUri, CancellationToken ct = default)
        {
            if (!TryDecodeDataUri(dataUri, _options.MaxImageBytes, out var bytes, out var type, out var error))
            {
                return new ImageSaveResult(false, null, error);
            }

            try
            {
                Directory.CreateDirectory(_root);
                var name = $"{Guid.NewGuid():N}.{type!.Extension}";           // a névben semmi nem jön a kliensről
                var path = Path.Combine(_root, name);
                await using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await fs.WriteAsync(bytes, ct);
                }
                return new ImageSaveResult(true, $"{RequestPath}/{name}", null);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(e, "A kép mentése nem sikerült a(z) {Root} mappába.", _root);
                return new ImageSaveResult(false, null, "A kép mentése nem sikerült a szerveren.");
            }
        }

        public async Task<SanitizedHtml> ProcessEditorHtmlAsync(string html, CancellationToken ct = default)
        {
            var warnings = new List<string>();
            if (string.IsNullOrWhiteSpace(html)) return new SanitizedHtml(string.Empty, warnings, false);
            if (html.Length > _options.MaxHtmlChars)
            {
                warnings.Add("A tartalom túl nagy, kérjük, rövidítsd (vagy kevesebb / kisebb képet használj).");
                return new SanitizedHtml(string.Empty, warnings, true);
            }

            var doc = _sanitizer.Sanitize(html, warnings, out var pending);
            if (doc is null) return new SanitizedHtml(string.Empty, warnings, true);
            if (pending.Count > MaxImagesPerSave)
            {
                warnings.Add($"Egyszerre legfeljebb {MaxImagesPerSave} új kép menthető.");
                return new SanitizedHtml(string.Empty, warnings, true);
            }

            foreach (var img in pending)
            {
                ct.ThrowIfCancellationRequested();
                var src = HtmlEntity.DeEntitize(img.GetAttributeValue("src", string.Empty));
                var result = await SaveDataUriAsync(src, ct);
                if (result.IsSuccess)
                {
                    img.SetAttributeValue("src", result.Url!);
                }
                else
                {
                    warnings.Add(result.Error!);
                    img.Remove();            // a hibás képet nem hagyjuk benne (régen base64-ként a DB-ben maradt)
                }
            }
            return new SanitizedHtml(doc.DocumentNode.InnerHtml, warnings, false);
        }

        public bool TryResolve(string fileName, out string physicalPath, out string contentType)
        {
            physicalPath = string.Empty;
            contentType = string.Empty;
            if (string.IsNullOrEmpty(fileName) || !FileNameRegex.IsMatch(fileName)) return false;

            var full = Path.GetFullPath(Path.Combine(_root, fileName));
            if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal) || !File.Exists(full)) return false;

            contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => string.Empty,
            };
            physicalPath = full;
            return contentType.Length > 0;
        }

        private static string FormatSize(int bytes) => bytes >= 1024 * 1024 ? $"{bytes / 1024.0 / 1024.0:0.#} MB" : $"{bytes / 1024} KB";

        /// <summary>data:image/&lt;típus&gt;;base64,&lt;adat&gt; szigorú feldolgozása. Nincs regex a bemeneten, nincs kliensoldali fájlnév.</summary>
        internal static bool TryDecodeDataUri(string dataUri, int maxBytes, out byte[] bytes, out ImageType? type, out string error)
        {
            bytes = [];
            type = null;
            error = string.Empty;

            var comma = dataUri.IndexOf(',');
            if (!dataUri.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) || comma < 0 || comma > 120)
            {
                error = "Nem támogatott képformátum (png, jpg, gif, webp engedélyezett).";
                return false;
            }

            var meta = dataUri[5..comma].Split(';', StringSplitOptions.TrimEntries);
            var declared = meta[0].ToLowerInvariant();
            if (!declared.Is("image/png", "image/jpeg", "image/jpg", "image/pjpeg", "image/gif", "image/webp"))
            {
                error = "Nem támogatott képformátum (png, jpg, gif, webp engedélyezett).";
                return false;
            }
            if (!meta.Skip(1).Any(p => p.Equals("base64", StringComparison.OrdinalIgnoreCase)))
            {
                error = "Hibás képadat.";
                return false;
            }

            var payload = dataUri.AsSpan(comma + 1).Trim();
            var maxBase64 = ((long)maxBytes + 2) / 3 * 4 + 16;            // méret előszűrés a dekódolás ELŐTT
            if (payload.Length > maxBase64)
            {
                error = $"A kép túl nagy (legfeljebb {FormatSize(maxBytes)}).";
                return false;
            }

            var buffer = new byte[(payload.Length / 4 + 1) * 3];
            if (!Convert.TryFromBase64Chars(payload, buffer, out var written) || written == 0)
            {
                error = "Hibás képadat.";
                return false;
            }
            if (written > maxBytes)
            {
                error = $"A kép túl nagy (legfeljebb {FormatSize(maxBytes)}).";
                return false;
            }

            var detected = ImageSniffer.Detect(buffer.AsSpan(0, written));
            if (detected is null)
            {
                error = "A fájl nem valódi kép (png, jpg, gif, webp engedélyezett).";
                return false;
            }
            if (!ImageSniffer.DeclaredMimeMatches(declared, detected))
            {
                error = "A kép típusa nem egyezik a tartalmával.";
                return false;
            }

            bytes = buffer.AsSpan(0, written).ToArray();
            type = detected;
            return true;
        }
    }

    internal static class StringExtensionsForImages
    {
        public static bool Is(this string value, params string[] options) => options.Contains(value, StringComparer.Ordinal);
    }
}
