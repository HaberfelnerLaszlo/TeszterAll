using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using TeszterLibrary.ViewModel;

namespace TeszterLibrary.Services
{
	/// <summary>
	/// A böngészőoldali jelfigyelő (wwwroot/JS/clientSignals.js) C# burkolója. Minden hiba csendben elnyelődik: a jelek
	/// csak kiegészítők, hiányuk sosem akadályozhatja a gyakorlást (pl. megszakadt kapcsolat, előrenderelés, tesztek).
	/// </summary>
	public sealed class ClientSignalsTracker(IJSRuntime js, ILogger<ClientSignalsTracker> logger) : IAsyncDisposable, IDisposable
	{
		public const string ModulePath = "./_content/TeszterLibrary/JS/clientSignals.js";

		private IJSObjectReference? _module;

		/// <summary>Figyelés indítása és nullázás (az oldal első renderelése után hívandó).</summary>
		public async Task StartAsync()
		{
			try
			{
				_module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
				if (_module is not null) await _module.InvokeVoidAsync("start");
			}
			catch (Exception e) when (IsInteropFailure(e))
			{
				logger.LogDebug(e, "A kliensjel-figyelő nem indult el.");
			}
		}

		/// <summary>Az előző lekérés óta gyűlt jelek (és nullázás); null, ha nem elérhető.</summary>
		public async Task<ClientSignals?> TakeAsync()
		{
			if (_module is null) return null;
			try
			{
				return await _module.InvokeAsync<ClientSignals?>("take");
			}
			catch (Exception e) when (IsInteropFailure(e))
			{
				logger.LogDebug(e, "A kliensjeleket nem sikerült lekérni.");
				return null;
			}
		}

		private static bool IsInteropFailure(Exception e) =>
			e is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException or OperationCanceledException;

		/// <summary>
		/// Szinkron lezárásnál (pl. tesztkörnyezet) a JS-modult nem lehet felszabadítani; a böngésző oldal bezárásakor
		/// úgyis eltűnik. A DI-konténer így szinkron és aszinkron lezárással is működik.
		/// </summary>
		public void Dispose() => _module = null;

		public async ValueTask DisposeAsync()
		{
			if (_module is null) return;
			try
			{
				await _module.DisposeAsync();
			}
			catch (Exception e) when (IsInteropFailure(e))
			{
				// a kapcsolat már megszakadt
			}
		}
	}
}
