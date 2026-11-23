namespace TeszterLibrary.ViewModel
{
	/// <summary>Jelvény-megerősítő kihívás indítása (Math/BadgeConfirm/{code}); a szerver BadgeConfirmStartView-ja.</summary>
	public class BadgeConfirmStartView
	{
		public string Code { get; set; } = string.Empty;
		public string BadgeName { get; set; } = string.Empty;
		/// <summary>A tanulónak szóló bevezető (pozitív hangvételű).</summary>
		public string Intro { get; set; } = string.Empty;
		/// <summary>Ennyi helyes válasz kell.</summary>
		public int Required { get; set; }
		/// <summary>A futás (a válaszok a szokásos Math/Challenge/Answer végponton mennek).</summary>
		public ChallengeStartView Run { get; set; } = new();
	}

	/// <summary>Jelvény-megerősítő kihívás eredménye (Math/BadgeConfirm/Finish/{runId}).</summary>
	public class BadgeConfirmResultView
	{
		public string Code { get; set; } = string.Empty;
		/// <summary>A jelvény megvan (megerősítve).</summary>
		public bool Passed { get; set; }
		public int Correct { get; set; }
		public int Required { get; set; }
		/// <summary>A tanulónak szóló üzenet (sikertelenségnél is barátságos).</summary>
		public string Message { get; set; } = string.Empty;
	}
}
