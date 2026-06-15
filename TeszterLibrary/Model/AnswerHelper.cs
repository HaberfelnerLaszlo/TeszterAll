namespace TeszterLibrary.Model
{
    public class AnswerHelper
    {
        public int Id { get; set; }
        /// <summary>
        /// A válasz azonosítója, melyhez a segítség tartozik.
        /// </summary>
        public int AnswerId { get; set; }
        /// <summary>
        /// A segítség szövege.
        /// </summary>
        public string Text { get; set; } = string.Empty;
        /// <summary>
        /// A segítség sorszáma.
        /// </summary>
        public int Sequence { get; set; }
    }
}
