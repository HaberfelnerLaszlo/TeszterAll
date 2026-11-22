namespace TeszterLibrary.ViewModel
{
	/// <summary>Egy Teams-csoport heti ranglistája a hívó szemszögéből.</summary>
	public class LeaderboardView
	{
		public string ClassId { get; set; } = string.Empty;
		public string ClassName { get; set; } = string.Empty;
		/// <summary>A hét első napja (hétfő), ettől számít a heti XP.</summary>
		public DateTime WeekStart { get; set; }
		/// <summary>A csoport diákjainak száma.</summary>
		public int Total { get; set; }
		/// <summary>A hívó helyezése (holtversenyben azonos); 0, ha nem diákja a csoportnak (pl. tanár).</summary>
		public int MyRank { get; set; }
		public int MyXp { get; set; }
		/// <summary>Van-e a listán a megjelenítettek fölött, illetve alatt további diák (diák nézetben "⋮" jelzéshez).</summary>
		public bool MoreAbove { get; set; }
		public bool MoreBelow { get; set; }
		/// <summary>Diáknak csak a hozzá közeli helyezések; tanárnak a teljes lista.</summary>
		public List<LeaderboardEntryView> Entries { get; set; } = new();
	}

	public class LeaderboardEntryView
	{
		public int Rank { get; set; }
		public string Name { get; set; } = string.Empty;
		public int Xp { get; set; }
		public bool IsMe { get; set; }
	}

	/// <summary>A hívó egy csoportja (a ranglista csoportválasztójához).</summary>
	public class LeaderboardClassView
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
	}
}
