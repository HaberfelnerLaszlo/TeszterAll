using System.ComponentModel.DataAnnotations.Schema;


namespace TeszterLibrary.Model
{/// <summary>
/// Egy adott felhasználó adott tesztjének adatai
/// </summary>
    public class TestUser
    {
        public int Id { get; set; }
        public int TestId { get; set; }

        public Guid UserId { get; set; }
        /// <summary>
        /// Próbálkozások száma. Mely minden próbálkozásnál eggyel nő.
        /// </summary>
        public int ReplaceNumber { get; set; }
        /// <summary>
        /// A teszttel végzett. Vagy a próbálkozások száma elérte a maximumot vagy letelt az időtartam.
        /// </summary>
        public bool IsReady { get; set; } = false;
        /// <summary>
        /// Első teszt időpontja
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Utolsó próbálkozás beadás időpontja
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// Próbálkozások közül a legjobb pontjai.
        /// </summary>
        public int Summa { get; set; }
        /// <summary>
        /// A teszt már aktív. Időzített teszt esetén hamis.
        /// </summary>
        public bool IsActive { get; set; } = false;
        public List<int> TestQuestionUserIds { get; set; } = new();
        public Test Test { get; set; }
    }
}
