using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeszterLibrary.Model
{
	/// <summary>
	/// Itt tárolódik, hogy a felhasználó az adott tesztkérdésre melyik próbálkozásnál milyen választ adott.
	/// Próbálkozásonként mentődik.
	/// </summary>
	public class TestQuestionUser
	{
		[Key]
		public int Id { get; set; }
		public int TestQuestionId { get; set; }
		[ForeignKey("TestUserId")]
		public int TestUserId { get; set; }
		/// <summary>
		/// Hányadik próbálkozás válasza
		/// </summary>
		public int ReplacedNumber { get; set; }
		/// <summary>
		/// Mikor rögzitette a választ.
		/// </summary>
		public DateTime AnswerTime { get; set; }
		/// <summary>
		/// A kérdés típusa. Nem mentődik az adatbázisba
		/// </summary>
		[NotMapped]
		public QuestionType QType { get; set; }
		/// <summary>
		/// Kérdésre kapott pont
		/// </summary>
		public int Score { get; set; }
		/// <summary>
		/// Automatikusan nem kiértékelhető volt a válasz. Alapértelmezett: HAMIS vagyis kiértékelhető.
		/// </summary>
		public bool IsRated { get; set; } = false;
		/// <summary>
		/// Mennyi idő alatt végzett.
		/// </summary>
		public int Time { get; set; }
		/// <summary>
		/// A válaszok -> ez volt! ; -vel elválasztva lesznek mentve.
		/// </summary>
		public List<TestAnswer>? Answers { get; set; }
		//[NotMapped]
		//public TestQuestion? TestQuestion { get; set; }
	}
}
