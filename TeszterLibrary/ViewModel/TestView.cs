using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.ViewModel
{
    public class TestView
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        /// <summary>
        /// Teszt készítőjének azonosítója.
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
        /// Kérdések mely tantárgyhoz kapcsolódnak
        /// </summary>
        public string Subject { get; set; }= String.Empty;

    }
}
