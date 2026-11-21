using System.ComponentModel.DataAnnotations;

namespace TeszterLibrary.Model
{
	public class Lesson
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public Guid StudentId { get; set; }
		public int TaskId { get; set; }
		public TaskList? TaskList { get; set; }
		/// <summary>
		/// Legalább ennyi feladatot meg kell oldani a továbblépés előtt (0 esetén 1). Lásd: LessonProgressRules.
		/// </summary>
		[Range(0, 10,ErrorMessage ="Az érték 0 és 10 között lehet.")]
		public byte MinCount { get; set; }
		/// <summary>
		/// Legfeljebb ennyi feladat; ha addig nem éri el a kért helyes arányt, visszalép (0 esetén 30).
		/// </summary>
		[Range(0, 30,ErrorMessage ="Az érték 0 és 30 között lehet.")]
		public byte MaxCount { get; set; }
		/// <summary>
		/// A hibás megoldások száma, mely után azonnal az előző leckére vált (0 esetén nem számít).
		/// </summary>
		[Range(0, 30, ErrorMessage = "Az érték 0 és 30 között lehet.")]
		public byte WrongCount { get; set; }
		/// <summary>
		/// A feladatok legalább ennyi %-ának jónak kell lennie a továbbhaladáshoz (0 esetén nem számít). A név ellenére a HELYES arány!
		/// </summary>
		[Range(0, 100,ErrorMessage ="Az érték 0 és 100 között lehet.")]
		public byte WrongPercent { get; set; }
		public int Time { get; set; }
		public bool IsCompleted { get; set; }
		public bool IsTipp { get; set; }
		public bool IsProcessing { get; set; }
		/// <summary>A lecke helye a tanuló feladatsorában (növekvő; 0 = a végére kerül).</summary>
		public int Order { get; set; }
	}
}
