namespace TeszterLibrary.ViewModel.Test
{
    public class TestResultToTeacher
    {
        /// <summary>
        /// A teszt azonosítója
        /// </summary>
        public int TestId { get; set; }
        /// <summary>
        /// A teszt címe.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Teszt kezdésének időpontja.
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Teszt befejezésének időpontja.
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// Teszt kitültök akik befejezték.
        /// </summary>
        public int ReadyNumber { get; set; }
        /// <summary>
        /// Ahány tanulónak ki lett osztva a teszt.
        /// </summary>
        public int UserNumber { get; set; }
        /// <summary>
        /// Tesztre fordítható időtartam.
        /// </summary>
        public int TestTime { get; set; }
        /// <summary>
        /// A tesztre fordított minimális időtartam.
        /// </summary>
        public int MinTime { get; set; }
        /// <summary>
        /// A tesztre fordított maximális időtartam.
        /// </summary>
        public int MaxTime { get; set; }
        /// <summary>
        /// A teszt gyakorlásra szolgál.
        /// </summary>
        public bool IsPractice { get; set; }
        /// <summary>
        /// Időt mérni kell?
        /// </summary>
        public bool IsTimer { get; set; }
        /// <summary>
        /// Láthatja a megoldást.
        /// </summary>
        public bool IsCheck { get; set; }
        /// <summary>
        /// Van nem pontozott megoldás.
        /// </summary>
        public bool HasNoDecided { get; set; }
        /// <summary>
        /// Próbálkozások száma a tesztnél.
        /// </summary>
        public int TestReplaceNumber { get; set; }
        /// <summary>
        /// Kérdések mely tantárgyhoz kapcsolódnak
        /// </summary>
        public string Subject { get; set; } = string.Empty;
        /// <summary>
        /// A teszthez rendelt felhasználó azonosító
        /// </summary>
        public int[]? TestUserIds { get; set; }


    }
}
