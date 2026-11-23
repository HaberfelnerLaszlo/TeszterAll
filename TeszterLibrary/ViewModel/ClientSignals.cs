namespace TeszterLibrary.ViewModel
{
	/// <summary>
	/// A böngésző jelei a válasszal együtt (a szerver ClientSignals-a). Csak számok és logikai értékek: a beírt vagy
	/// beillesztett szöveg tartalma soha. A szerver csak gyenge, korlátos kiegészítésként használja.
	/// </summary>
	public class ClientSignals
	{
		/// <summary>Mennyi ideig volt a lap háttérben / fókusz nélkül a feladat alatt (ms).</summary>
		public int FocusLossMs { get; set; }
		/// <summary>Volt-e beillesztés a válaszmezőbe.</summary>
		public bool Paste { get; set; }
		/// <summary>navigator.webdriver === true.</summary>
		public bool WebDriver { get; set; }
		/// <summary>Volt-e nem megbízható (isTrusted == false) esemény.</summary>
		public bool UntrustedEvent { get; set; }
		/// <summary>Nem volt sem egér/érintés, sem billentyű esemény.</summary>
		public bool NoInputEvents { get; set; }
	}
}
