using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeszterLibrary.Model
{
	public class Question
	{
		[Key]
		public Guid Id { get; set; }
		[Required, MaxLength(250)]
		public string Title { get; set; } =string.Empty;
		[MaxLength(50)]
		public string Subtitle { get; set; } =string.Empty;
		public string Text { get; set; } =string.Empty;
		[Required, Range(0,100,ErrorMessage ="Csak 0 és 100 közötti érték adható!")]
		public int Score { get; set; }
		public int Time { get; set; }
		[Range(1,12,ErrorMessage ="Csak 1 és 12 közötti érték adható!")]
		public int MinGrade { get; set; }
		[Range(1, 12, ErrorMessage = "Csak 1 és 12 közötti érték adható!")]
		public int MaxGrade { get; set; }
		[MaxLength(50)]
		public string Subject { get; set; } = string.Empty;
		public QuestionType QType { get; set; }
		/// <summary>
		/// Kérdés készítőjének azonosítója.
		/// </summary>
		public Guid CreatedBy { get; set; }
		[NotMapped]
		public List<Answer> Answers { get; set; }
	}
	public enum QuestionType {All,SC,MC,TF,Open,Seq,Pair,Comp }
}
