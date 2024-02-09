namespace TeszterLibrary.ViewModel.Test
{
    public class UserResultToTeacher
    {
        /// <summary>
        /// A felhasználó neve
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// A próbálozások közül a legrövidebb idő. Ha egy próbálozás volt ez nulla.
        /// </summary>
        public int MinTime { get; set; } = 0;
        /// <summary>
        /// A próbálozások közül a leghosszabb idő.
        /// </summary>
        public int MaxTime { get; set; } = 0;
        /// <summary>
        /// A próbálkozások közül a legkevesebb pont. Ha egy próbálozás volt ez nulla.
        /// </summary>
        public int MinScore { get; set; } = 0;
        /// <summary>
        /// A próbálkozások közül a legtöbb pont.
        /// </summary>
        public int MaxScore { get; set; } = 0;
        /// <summary>
        /// Hány próbálkozás volt.
        /// </summary>
        public int ReplaceNumber { get; set; } = 0;
        /// <summary>
        /// Az automatikusan nem pontozható kérdések azonosítói. TestQuestionUserId
        /// </summary>
        public List<int>? NoDecided { get; set; }
        /// <summary>
        /// Az összes próbálkozás összes válasza.
        /// </summary>
        public List<QuestionResult> UserAnswers { get; set; }=new List<QuestionResult>();
    }
}
