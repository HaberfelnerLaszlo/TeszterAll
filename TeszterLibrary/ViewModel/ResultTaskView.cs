namespace TeszterLibrary.ViewModel
{
	public class ResultTaskView
	{
		public int PublicCalcId { get; set; }
		public string Sequence { get; set; } = string.Empty; 
		public string Outcome { get; set; } = string.Empty;
		public string Answer { get; set; } = string.Empty;
		/// <summary>
		/// Mennyi idő alatt oldotta meg
		/// </summary>
		public int Time { get; set; }
		/// <summary>
		/// A válasz helyessége.
		/// </summary>
		public bool IsCorrect { get; set; }
		/// <summary>
		/// Kapott hozzá választási lehetőséget
		/// </summary>
		public bool IsTipp { get; set; }
		public DateTime TimeStamp { get; set; } = DateTime.Now;

	}
}
