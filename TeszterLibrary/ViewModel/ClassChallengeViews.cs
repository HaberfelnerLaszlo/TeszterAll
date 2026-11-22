namespace TeszterLibrary.ViewModel
{
	/// <summary>Osztály-kihívás indítása (tanár).</summary>
	public class ClassChallengeRequest
	{
		public string ClassId { get; set; } = string.Empty;
		public int TaskId { get; set; }
		/// <summary>Hossz mp-ben (30–300).</summary>
		public int Seconds { get; set; } = 60;
	}

	/// <summary>Egy osztály-kihívás és élő állása.</summary>
	public class ClassChallengeView
	{
		public int Id { get; set; }
		public string ClassName { get; set; } = string.Empty;
		public string TopicName { get; set; } = string.Empty;
		/// <summary>0 gyülekező, 1 folyamatban, 2 véget ért.</summary>
		public byte State { get; set; }
		/// <summary>Kezdésig hátralévő mp (gyülekezőben).</summary>
		public int SecondsToStart { get; set; }
		/// <summary>Végéig hátralévő mp.</summary>
		public int SecondsLeft { get; set; }
		/// <summary>Csatlakozott diákok száma.</summary>
		public int Participants { get; set; }
		/// <summary>A hívó csatlakozott-e (diák).</summary>
		public bool Joined { get; set; }
		/// <summary>A hívó helyezése (0, ha nem játszik).</summary>
		public int MyRank { get; set; }
		public int MyCorrect { get; set; }
		/// <summary>Tanárnak a teljes lista; diáknak csak a hozzá közeli helyezések.</summary>
		public List<LeaderboardEntryView> Entries { get; set; } = new();
		public bool MoreAbove { get; set; }
		public bool MoreBelow { get; set; }
	}
}
