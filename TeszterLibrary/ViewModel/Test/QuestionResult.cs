namespace TeszterLibrary.ViewModel.Test
{
    public class QuestionResult
    {
        /// <summary>
        /// A TestQuestionUserId a beazonosításhoz módosítás esetén.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// A teszt kérdés azonosítója
        /// </summary>
        public int TestQuestionId { get; set; }
        /// <summary>
        /// Kérdésre kapható pontok. Vagy viszza fele a kérdésre kapott pontok.
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// A kérdésre fordítható idő
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// A válaszok melyik próbálkozásnál voltak.
        /// </summary>
        public int ReplaceN {  get; set; }
        /// <summary>
        /// A válaszok.
        /// </summary>
        public List<AnswerView>? Answers { get; set; }

    }
}
