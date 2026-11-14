namespace TeszterLibrary.Services.Images
{
    /// <summary>Konfig szekció: "Uploads" (appsettings.json).</summary>
    public sealed class ImageStorageOptions
    {
        public const string SectionName = "Uploads";

        /// <summary>
        /// A képek tárolási mappája. Alapértelmezés: &lt;ContentRoot&gt;/App_Data/uploads, vagyis a wwwroot-on KÍVÜL,
        /// így a statikus fájlszerver nem szolgálja ki közvetlenül (csak a hitelesített végponton át).
        /// Dockerben ide kötetet (volume) csatolj, különben újratelepítéskor elvesznek a képek.
        /// </summary>
        public string? StoragePath { get; set; }

        /// <summary>Egy kép legnagyobb mérete bájtban (kicsomagolva). Alapértelmezés: 2 MB.</summary>
        public int MaxImageBytes { get; set; } = 2 * 1024 * 1024;

        /// <summary>A kiszolgáló végpont útvonala. A régi képek miatt marad "/uploads".</summary>
        public string RequestPath { get; set; } = "/uploads";

        /// <summary>A szerkesztőből érkező teljes HTML legnagyobb hossza karakterben.</summary>
        public int MaxHtmlChars { get; set; } = 4_000_000;
    }
}
