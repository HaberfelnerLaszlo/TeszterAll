namespace TeszterLibrary.ViewModel
{
	/// <summary>Egy tanuló állapota a tanári csoport-áttekintőben.</summary>
	public class ResultClassView
	{
		public Guid StudentId { get; set; }
		public string StudentName { get; set; } = string.Empty;
		/// <summary>Teljesített leckék száma (régen: az összes próbálkozásé, a visszalépettekkel együtt).</summary>
		public int Complet { get; set; }
		/// <summary>Visszalépéssel végződött próbálkozások.</summary>
		public int Back { get; set; }
		/// <summary>Félbehagyott próbálkozások.</summary>
		public int Abandoned { get; set; }
		/// <summary>Hátralévő (nem teljesített) leckék.</summary>
		public int NoComplet { get; set; }
		/// <summary>Az összes kiadott lecke; 0 = nincs feladatsora.</summary>
		public int Total { get; set; }
		/// <summary>Az aktuális lecke helye a sorban (1-től); 0, ha mindet teljesítette vagy nincs feladatsora.</summary>
		public int Position { get; set; }
		/// <summary>Az aktuális lecke neve (üres, ha nincs).</summary>
		public string CurrentLesson { get; set; } = string.Empty;
		/// <summary>Az aktuális leckén épp folyik próbálkozás.</summary>
		public bool InProgress { get; set; }
		/// <summary>Hány napja válaszolt utoljára; null, ha még egyszer sem (régen ilyenkor kimaradt a listából).</summary>
		public int? LastActive { get; set; }
	}
}
