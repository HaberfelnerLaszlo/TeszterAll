namespace TeszterLibrary.ViewModel
{
	/// <summary>A tanuló játékos haladása: XP, szint, napi sorozat, jelvények, pályatérkép.</summary>
	public class StudentProgressView
	{
		public int Xp { get; set; }
		public int Level { get; set; }
		/// <summary>Az aktuális szint kezdete (XP).</summary>
		public int LevelStartXp { get; set; }
		/// <summary>A következő szint kezdete (XP).</summary>
		public int NextLevelXp { get; set; }
		/// <summary>Egymást követő napok válasszal, ma vagy tegnap bezárólag (0, ha megszakadt).</summary>
		public int Streak { get; set; }
		/// <summary>A valaha elért leghosszabb sorozat.</summary>
		public int BestStreak { get; set; }
		/// <summary>Az összes jelvény; az Earned mondja meg, melyik van már meg.</summary>
		public List<BadgeView> Badges { get; set; } = new();
		/// <summary>A leckesor a feladatsor sorrendjében.</summary>
		public List<MapStepView> Map { get; set; } = new();
	}

	public class BadgeView
	{
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public bool Earned { get; set; }
		/// <summary>A feltétel teljesült, de a jelvény egy rövid megerősítő kihívásra vár (/student/badge/{Code}).</summary>
		public bool Pending { get; set; }
	}

	public class MapStepView
	{
		public int Position { get; set; }
		public string Name { get; set; } = string.Empty;
		/// <summary>0 hátralévő, 1 aktuális, 2 teljesített.</summary>
		public byte State { get; set; }
		/// <summary>Gyakorló teszt (vegyes feladatok).</summary>
		public bool IsPractice { get; set; }
	}
}
