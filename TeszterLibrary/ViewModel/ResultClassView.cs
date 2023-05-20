namespace TeszterLibrary.ViewModel
{
	public class ResultClassView
	{
		public Guid StudentId { get; set; }
		public string StudentName { get; set; } = string.Empty;
		public int Complet { get; set; }
		public int Back { get; set; }
		public int NoComplet { get; set; }
		public int LastActive { get; set;}
		//public List<int> CompletTaskIds	{ get; set; } = new List<int>();
	}
}
