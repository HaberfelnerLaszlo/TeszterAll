namespace TeszterLibrary.ViewModel
{
	/// <summary>Választható téma az időre menő kihíváshoz (0 = vegyes), a saját rekorddal.</summary>
	public class ChallengeTopicView
	{
		public int TaskId { get; set; }
		public string Name { get; set; } = string.Empty;
		/// <summary>Eddigi legtöbb helyes válasz ebben a témában (0, ha még nem próbálta).</summary>
		public int Best { get; set; }
	}

	/// <summary>Elindított kihívás: a feladatok és az idő.</summary>
	public class ChallengeStartView
	{
		public int RunId { get; set; }
		public int Seconds { get; set; }
		/// <summary>Kezdés (UTC); osztály-kihívásnál a gyülekező vége, addig nem lehet válaszolni.</summary>
		public DateTime StartsAt { get; set; }
		/// <summary>Vége (UTC), a szerver órája szerint.</summary>
		public DateTime EndsAt { get; set; }
		public List<OperationView> Operations { get; set; } = new();
	}

	/// <summary>Egy kihívásbeli válasz beküldése.</summary>
	public class ChallengeAnswerRequest
	{
		public int RunId { get; set; }
		public int OperationId { get; set; }
		public string Result { get; set; } = string.Empty;
	}

	/// <summary>A válasz kiértékelése és az állás.</summary>
	public class ChallengeAnswerView
	{
		public bool IsCorrect { get; set; }
		public int Correct { get; set; }
		public int Wrong { get; set; }
		/// <summary>Hátralévő idő (mp) a szerver szerint.</summary>
		public int SecondsLeft { get; set; }
		/// <summary>Lejárt az idő: ez a válasz már nem számított.</summary>
		public bool Expired { get; set; }
	}

	/// <summary>A kihívás eredménye.</summary>
	public class ChallengeResultView
	{
		public int Correct { get; set; }
		public int Wrong { get; set; }
		public int Xp { get; set; }
		/// <summary>A rekord ebben a témában (ezzel a próbálkozással együtt).</summary>
		public int Best { get; set; }
		/// <summary>Új egyéni rekord.</summary>
		public bool IsRecord { get; set; }
	}
}
