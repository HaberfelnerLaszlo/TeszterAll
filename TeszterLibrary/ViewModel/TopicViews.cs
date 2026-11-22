namespace TeszterLibrary.ViewModel
{
	/// <summary>Egy téma (TaskList + Sablon) a tanári szerkesztőben.</summary>
	public class TopicEditView
	{
		/// <summary>0 = új téma.</summary>
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Topic { get; set; } = string.Empty;
		/// <summary>Natural, Integer, Fraction, Units, …; üres = nincs generátor (gyakorló teszt).</summary>
		public string GeneratorKind { get; set; } = string.Empty;
		/// <summary>A generátor paraméterei JSON-ban.</summary>
		public string GeneratorParams { get; set; } = string.Empty;
		public byte MinCount { get; set; } = 3;
		public byte MaxCount { get; set; } = 10;
		public byte WrongCount { get; set; } = 5;
		public byte WrongPercent { get; set; } = 70;
		public int Time { get; set; }
	}

	/// <summary>Előnézet kérése: a generátor és a paraméterei.</summary>
	public class TopicPreviewRequest
	{
		public string GeneratorKind { get; set; } = string.Empty;
		public string GeneratorParams { get; set; } = string.Empty;
	}

	/// <summary>Előnézet: mintafeladatok vagy a hiba oka.</summary>
	public class TopicPreviewView
	{
		public bool Ok { get; set; }
		public string Error { get; set; } = string.Empty;
		public List<TopicSampleView> Samples { get; set; } = new();
	}

	public class TopicSampleView
	{
		/// <summary>A feladat szövege, a válasz helyén "?"-lel.</summary>
		public string Text { get; set; } = string.Empty;
		public string Outcome { get; set; } = string.Empty;
	}
}
