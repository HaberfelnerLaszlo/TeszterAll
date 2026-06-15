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
        /// <summary>
        /// A helyes válasz szövege (kiegészítésnél)
        /// </summary>
        public string RightAnswer { get; set; } = String.Empty;
        /// <summary>
        /// Helyes válasz? Vagy párosításnál a második false az első true legyen.
        /// </summary>
        public bool Right { get; set; }
		/// <summary>
		/// Sorbarendezésnél a sorrend vagy párosításnál a pár száma vagy kiegészítésnél hányadik
		/// </summary>
		public int Sequence { get; set; }
        [Range(0, 100, ErrorMessage = "Csak 0 és 100 közötti érték adható!")]
        public int Score { get; set; } = 1;
        public List<AnswerHelper>? AnswerHelpers { get; set; }
    }
}
