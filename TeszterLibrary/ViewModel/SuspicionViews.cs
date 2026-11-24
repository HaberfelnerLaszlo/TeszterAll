namespace TeszterLibrary.ViewModel
{
	/// <summary>Egy csoport tanulói a gyanújelzések szerint (tanári nézet). Jelzés, nem bizonyíték.</summary>
	public class SuspicionClassView
	{
		public string ClassId { get; set; } = string.Empty;
		public string ClassName { get; set; } = string.Empty;
		public int WindowDays { get; set; }
		/// <summary>Szint szerint csökkenő, azon belül pontszám, majd név szerint.</summary>
		public List<SuspicionStudentRow> Students { get; set; } = new();
	}

	public class SuspicionStudentRow
	{
		public Guid StudentId { get; set; }
		public string Name { get; set; } = string.Empty;
		/// <summary>0 zöld, 1 sárga, 2 piros.</summary>
		public byte Level { get; set; }
		public int Score { get; set; }
		/// <summary>Az időablakban számító jelzések száma.</summary>
		public int SignalCount { get; set; }
		/// <summary>Megerősítésre váró jelvények száma.</summary>
		public int PendingBadges { get; set; }
		/// <summary>A legtöbb pontot adó jelzésfajták érthető neve.</summary>
		public List<string> MainReasons { get; set; } = new();
	}

	/// <summary>Egy tanuló jelzései és jelvényállapotai (tanári nézet).</summary>
	public class SuspicionStudentView
	{
		public Guid StudentId { get; set; }
		public string Name { get; set; } = string.Empty;
		public byte Level { get; set; }
		public int Score { get; set; }
		public int ServerScore { get; set; }
		public int ClientScore { get; set; }
		public int YellowThreshold { get; set; }
		public int RedThreshold { get; set; }
		public int WindowDays { get; set; }
		public int RetentionDays { get; set; }
		/// <summary>A tárolt jelzések, legújabb elöl.</summary>
		public List<SuspicionSignalView> Signals { get; set; } = new();
		/// <summary>A jelvények, amelyeknek van tárolt állapota (függőben, megerősített, elutasított, megszerzett).</summary>
		public List<BadgeStateView> Badges { get; set; } = new();
	}

	public class SuspicionSignalView
	{
		public int Id { get; set; }
		public byte Type { get; set; }
		/// <summary>A jelzés fajtájának érthető neve.</summary>
		public string Title { get; set; } = string.Empty;
		/// <summary>Mit mért a rendszer és mihez hasonlította (mondatban, nem nyers adatként).</summary>
		public string Description { get; set; } = string.Empty;
		public int Weight { get; set; }
		/// <summary>Kliensoldali (gyenge, kis súlyú) jelzés.</summary>
		public bool IsClient { get; set; }
		public DateTime CreatedAt { get; set; }
		/// <summary>A tanár „rendben, nem gond” jelölése: nem számít a pontszámba.</summary>
		public bool Dismissed { get; set; }
		/// <summary>Beleszámít-e most a pontszámba (időablakon belül és nincs elfogadva).</summary>
		public bool Counted { get; set; }
	}

	public class BadgeStateView
	{
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		/// <summary>1 megszerzett, 2 függőben, 3 megerősített, 4 elutasított (BadgeStatus).</summary>
		public byte Status { get; set; }
		public string StatusText { get; set; } = string.Empty;
		public DateTime FirstMetAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		/// <summary>Tanár döntött róla.</summary>
		public bool DecidedByTeacher { get; set; }
	}
}
