namespace TeszterLibrary.ViewModel.Test
{
    public class TestResultToStudent
    {
        /// <summary>
        /// A teszt azonosítója.
        /// </summary>
        //public int Id { get; set; }
        /// <summary>
        /// A teszt címe.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Teszt befejezésének időpontja.
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// Teszt összes pont.
        /// </summary>
        public int Summa { get; set; }
        /// <summary>
        /// A megszerzett össszes pont.
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// Tesztre fordítható időtartam.
        /// </summary>
        public int TestTime { get; set; }
        /// <summary>
        /// A tesztre fordított időtartam.
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// A teszt gyakorlásra szolgál.
        /// </summary>
        public bool IsPractice { get; set; }
        /// <summary>
        /// Próbálkozások száma a tesztnél.
        /// </summary>
        public int TestReplaceNumber { get; set; }
        /// <summary>
        /// Felhasználó próbálkozásainak száma.
        /// </summary>
        public int UserReplaceNumber { get; set; }
        /// <summary>
        /// Időt mérni kell?
        /// </summary>
        public bool IsTimer { get; set; }
        /// <summary>
        /// Kérdések mely tantárgyhoz kapcsolódnak
        /// </summary>
        public string Subject { get; set; } = string.Empty;
        /// <summary>
        /// A teszthez rendelt felhasználó azonosító
        /// </summary>
        public int TestUserId { get; set; }


    }
}
