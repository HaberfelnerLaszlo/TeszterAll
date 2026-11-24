using Bunit;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

using TeszterLibrary.Services;
using TeszterLibrary.ViewModel;

namespace TeszterTests.Services
{
	/// <summary>A kliensjel-figyelő: hibatűrés, és hogy a böngészőoldali kód csak számokat gyűjt (tartalmat soha).</summary>
	public class ClientSignalsTrackerTests : BunitContext
	{
		[Fact]
		public async Task Returns_the_signals_from_the_browser_module()
		{
			var module = JSInterop.SetupModule(ClientSignalsTracker.ModulePath);
			module.SetupVoid("start").SetVoidResult();
			module.Setup<ClientSignals?>("take").SetResult(new ClientSignals { FocusLossMs = 1200, Paste = true });

			var tracker = new ClientSignalsTracker(JSInterop.JSRuntime, NullLogger<ClientSignalsTracker>.Instance);
			await tracker.StartAsync();
			var signals = await tracker.TakeAsync();

			Assert.NotNull(signals);
			Assert.Equal(1200, signals!.FocusLossMs);
			Assert.True(signals.Paste);
		}

		[Fact]
		public async Task Missing_or_failing_javascript_never_breaks_practice()
		{
			// Nincs modul (pl. előrenderelés, tiltott JS): null jelek, kivétel nélkül.
			JSInterop.Mode = JSRuntimeMode.Loose;
			var tracker = new ClientSignalsTracker(JSInterop.JSRuntime, NullLogger<ClientSignalsTracker>.Instance);
			await tracker.StartAsync();
			Assert.Null(await tracker.TakeAsync());
			await tracker.DisposeAsync();
		}

		[Fact]
		public async Task Javascript_errors_are_swallowed()
		{
			var module = JSInterop.SetupModule(ClientSignalsTracker.ModulePath);
			module.SetupVoid("start").SetVoidResult();
			module.Setup<ClientSignals?>("take").SetException(new JSException("hiba"));

			var tracker = new ClientSignalsTracker(JSInterop.JSRuntime, NullLogger<ClientSignalsTracker>.Instance);
			await tracker.StartAsync();
			Assert.Null(await tracker.TakeAsync());
		}

		[Fact]
		public void The_browser_script_collects_no_content()
		{
			var dir = new DirectoryInfo(AppContext.BaseDirectory);
			while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TeszterAll.sln"))) dir = dir.Parent;
			Assert.NotNull(dir);
			var script = File.ReadAllText(Path.Combine(dir!.FullName, "TeszterLibrary", "wwwroot", "JS", "clientSignals.js"));

			// Adatminimalizálás: nem olvas vágólapot, beírt értéket, leütött billentyűt; nem rögzít képernyőt/kamerát.
			foreach (var forbidden in new[] { "clipboardData", "getData", ".value", "e.key", "event.key", "getUserMedia", "getDisplayMedia", "localStorage", "fetch(" })
				Assert.DoesNotContain(forbidden, script);
		}
	}
}
