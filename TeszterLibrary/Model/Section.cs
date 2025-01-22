namespace TeszterLibrary.Model
{
    public class Section
    {
        /// <summary>
        /// A szakasz azonosítója.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// A szakasz elnevezése.
        /// </summary>
        public string Name { get; set; }=string.Empty;
        /// <summary>
        /// Melyik tesztnél van használva.
        /// </summary>
        public int TestId { get; set; }
        /// <summary>
        /// A szakasz létrehozója
        /// </summary>
        public Guid CreatedBy { get; set; }
        /// <summary>
        /// Hány kérdés lesz kiadva. Ha nulla akkor a szakasz összes kérdése.
        /// </summary>
        public int Count { get; set; }
    }
}
