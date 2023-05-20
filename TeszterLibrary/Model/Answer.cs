using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeszterLibrary.Model
{
	public class Answer
	{
		[Key]
		public int Id { get; set; }
		/// <summary>
		/// Melyik kérdéshez tartozik.
		/// </summary>
		[Required]
		public Guid QuestionId { get; set; }
		/// <summary>
		/// A válasz
		/// </summary>
		[Required, MaxLength(250)]
		public string Text { get; set; }=string.Empty;
		//public byte[] Image { get; set; }
		/// <summary>
		/// Helyes válasz?
		/// </summary>
		public bool Right { get; set; }
		/// <summary>
		/// Sorbarendezésnél a sorrend vagy párosításnál a pár száma vagy kiegészítésnél hányadik
		/// </summary>
		public int Sequence { get; set; }
		public Question Question { get; set; }
	}
}
