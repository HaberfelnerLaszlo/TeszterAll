using System.ComponentModel.DataAnnotations;

namespace TeszterLibrary.Model
{
    public class Test
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="A cím nem lehet üres!")]
        public string Title { get; set; } = "Teszt_" + DateTime.Today.ToShortDateString();
        /// <summary>
        /// Teszt készítőjének azonosítója.
        /// </summary>
        public Guid CreatedBy { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTimeOffset.Now.AddDays(1).DateTime;
        /// <summary>
        /// Összes pont
        /// </summary>
        public int Summa { get; set; }
        /// <summary>
        /// Tesztre fordítható időtartam
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// A teszt gyakorlásra szolgál.
        /// </summary>
        public bool IsPractice { get; set; } = true;
        /// <summary>
        /// Próbálkozások száma.
        /// </summary>
        [Range(1,50,ErrorMessage ="Az értéknek 1 és 50 között kell lennie!")]
        public int ReplaceNumber { get; set; } = 5; //TODO RNumber maximalizálása itt is!
        /// <summary>
        /// Teszt közzétéve
        /// </summary>
        public bool IsPublished { get; set; } = false;
        /// <summary>
        /// A teszt szeciókra van osztva. A szekciókon belül lehet több kérdésből választani.
        /// </summary>
        public bool HasSection { get; set; } = false;
        /// <summary>
        /// Időt mérni kell?
        /// </summary>
        public bool IsTimer { get; set; } = true;
        /// <summary>
        /// Láthatja a megoldást.
        /// </summary>
        public bool IsCheck { get; set; } = false;
        /// <summary>
        /// Össze keveri a kérdéseket.
        /// </summary>
        public bool ShuffleQuestion { get; set; } = true;
        /// <summary>
        /// Kérdések mely tantárgyhoz kapcsolódnak
        /// </summary>
        [Required(ErrorMessage ="A tantárgy nem lehet üres, állítson be egyéb értéket ha vegyesen szeretné.")]
        public string Subject { get; set; } = string.Empty; //Todo: későbbiekben tantárgy alapján lehet sorbarendezni, kategorizálni.
        public List<TestQuestion> TestQuestions { get; set; } = new();
        public List<TestUser> TestUsers { get; set; } = new();
}
}
