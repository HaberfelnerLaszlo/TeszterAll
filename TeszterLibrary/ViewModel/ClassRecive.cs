namespace TeszterLibrary.ViewModel
{
	public class ClassRecive
	{
		public int Id { get; set; } = 0;
		public Guid ClassId { get; set; }
		public Guid StudentId { get; set; }
		public int OperationId { get; set; }
		/// <summary>
		/// A tanuló válasza
		/// </summary>
		public string Result { get; set; } = string.Empty;
		/// <summary>
		/// Mennyi idő alatt oldotta meg
		/// </summary>
		public int Time { get; set; }
		/// <summary>
		/// Kapott hozzá választási lehetőséget
		/// </summary>
		public bool IsTipp { get; set; }
	}
}
