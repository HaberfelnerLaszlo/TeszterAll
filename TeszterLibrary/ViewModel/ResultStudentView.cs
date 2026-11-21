namespace TeszterLibrary.ViewModel
{
	/// <summary>Egy lecke-próbálkozás a tanuló eredménylistájában.</summary>
	public class ResultStudentView
	{
		public int CompletTaskId { get; set; }
		public string TaskName { get; set; } = string.Empty;
		public int Right { get; set; }
		public int Wrong { get; set; }
		/// <summary>Az utolsó válasz ideje (DateTime.MinValue, ha nem volt válasz).</summary>
		public DateTime Date { get; set; }
		/// <summary>Átlagos megoldási idő mp-ben (régen az összidő volt).</summary>
		public int AvgTime { get; set; }
		/// <summary>0 -> továbblépett; 1 -> visszalépett (régi kliensekhez; lásd Status).</summary>
		public byte TaskType { get; set; }
		/// <summary>0 folyamatban, 1 teljesítve, 2 visszalépett, 3 félbehagyta (CompletTaskStatus).</summary>
		public byte Status { get; set; }
	}
}
