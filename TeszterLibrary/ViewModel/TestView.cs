using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.ViewModel
{
    /// <summary>
    /// Diak tesztek lekérdezésének az alapja.
    /// </summary>
    public class TestView
    {
        /// <summary>
        /// A teszt azonosítója.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// A teszt címe.
        /// </summary>
        public string Title { get; set; } = String.Empty;
        /// <summary>
        /// Teszt befejezésének időpontja.
        /// </summary>
        public DateTime EndTime { get; set; }
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
        public string Subject { get; set; }= String.Empty;
        /// <summary>
        /// A teszthez rendelt felhasználó azonosító
        /// </summary>
        public int TestUserId { get; set; }

    }
}
