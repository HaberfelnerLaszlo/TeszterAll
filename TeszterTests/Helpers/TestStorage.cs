using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using TeszterLibrary.Services.Images;

namespace TeszterTests.Helpers
{
    /// <summary>Ideiglenes mappába író ImageStorage; a Dispose letörli a mappát.</summary>
    public sealed class TestStorage : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "teszter-tests-" + Guid.NewGuid().ToString("N"));
        public ImageStorage Storage { get; }

        public TestStorage(int maxImageBytes = 2 * 1024 * 1024)
        {
            Storage = new ImageStorage(
                Options.Create(new ImageStorageOptions { StoragePath = Root, MaxImageBytes = maxImageBytes }),
                DefaultsProxy.Create<IWebHostEnvironment>(),   // a StoragePath meg van adva, a környezetet nem használja
                NullLogger<ImageStorage>.Instance);
        }

        public int FileCount => Directory.Exists(Root) ? Directory.GetFiles(Root).Length : 0;

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
