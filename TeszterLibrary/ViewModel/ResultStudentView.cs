namespace TeszterLibrary.ViewModel
{
	public class ResultStudentView
	{
		public int CompletTaskId { get; set; }
		public string TaskName { get; set; } = string.Empty;
		public int Right { get; set; }
		public int Wrong { get; set; }
		public DateTime Date { get; set; } //Sorrend miatt
		public int AvgTime { get; set; }
		public byte TaskType { get; set; }// 0 -> tovább lépet; 1 -> vissza lépet
		//public List<int> PublicCalcIds { get; set; } = new();
	}
}
