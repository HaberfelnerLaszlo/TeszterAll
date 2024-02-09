using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeszterLibrary.Model
{
	/// <summary>
	/// Az adott teszt felhasználó válaszait menti
	/// </summary>
	public class TestAnswer
	{       
		/// <summary>
		/// Egyedi azonosító
		/// </summary>
		[Key]
		public int Id { get; set; }
		/// <summary>
		/// Melyik válaszra lett adva.
		/// </summary>
		[Required]
		public int AnswerId { get; set; }
		/// <summary>
		/// A válasz szövege ha nyílt végű és kiegészítendős
		/// </summary>
		public string? Text { get; set; }
		///// <summary>
		///// Ha a válasz kép.
		///// </summary>
		//public byte[]? Image { get; set; }
		///// <summary>
		///// Megjelölt válasz.
		///// </summary>
		public bool Sign { get; set; }
		/// <summary>
		/// Az adott válasz helyes volt-e.
		/// </summary>
		public bool Result { get; set; }=false;
		/// <summary>
		/// Sorbarendezésnél a sorrend vagy párosításnál a pár száma
		/// </summary>
		public int Sequence { get; set; }
		/// <summary>
		/// Melyik teszt kérdéshez tartozik.
		/// </summary>		[Required]
		public int QuestionId { get; set; }
		[Required, ForeignKey("TestQuestionUserID")]
		public int TestQuestionUserId { get; set; }
		//public virtual TestQuestionUser? TestQuestionUser { get; set; }
	}
}
