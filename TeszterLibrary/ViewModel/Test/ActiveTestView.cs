namespace TeszterLibrary.ViewModel.Test
{
    public class ActiveTestView: Model.Test
    {
        /// <summary>
        /// A tesztel végzők száma
        /// </summary>
        public int Ready { get; set; }
        /// <summary>
        /// A tesztet még nem kezdték meg.
        /// </summary>
        public int NotActive { get; set; }
        /// <summary>
        /// Az összes teszt író létszáma.
        /// </summary>
        public int All { get; set; }
    }
}
