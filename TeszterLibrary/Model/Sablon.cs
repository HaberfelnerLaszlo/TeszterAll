using System.ComponentModel.DataAnnotations;

namespace TeszterLibrary.Model
{
	public class Sablon
	{
		[Key]
		public int TaskId { get; set; }
		public TaskList? TaskList { get; set; }
		/// <summary>
		/// Legalább ennyi feladatot meg kell oldani a továbblépés előtt (0 esetén 1). Lásd: LessonProgressRules.
		/// </summary>
		[Range(0, 10, ErrorMessage = "Az érték 0 és 10 között lehet.")]
		public int MinCount { get; set; }
		/// <summary>
		/// Legfeljebb ennyi feladat; ha addig nem éri el a kért helyes arányt, visszalép (0 esetén 30).
		/// </summary>
		[Range(0, 30, ErrorMessage = "Az érték 0 és 30 között lehet.")]
		public int MaxCount { get; set; }
		/// <summary>
		/// A hibás megoldások száma, mely után azonnal az előző leckére vált (0 esetén nem számít).
		/// </summary>
		[Range(0, 30, ErrorMessage = "Az érték 0 és 30 között lehet.")]
		public int WrongCount { get; set; }
		/// <summary>
		/// A feladatok legalább ennyi %-ának jónak kell lennie a továbbhaladáshoz (0 esetén nem számít). A név ellenére a HELYES arány!
		/// </summary>
		[Range(0, 100, ErrorMessage = "Az érték 0 és 100 között lehet.")]
		public int WrongPercent { get; set; }
		public int Time { get; set; }
	}
}
