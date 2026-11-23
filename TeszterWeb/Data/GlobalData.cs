using TeszterLibrary.Model;

using TeszterWeb.Components.Pages.Teacher;

namespace TeszterWeb.Data
{
    public class GlobalData
    {
        private string? _userId = string.Empty;

        public string? UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                //Userchangedevent?.Invoke();
            }
        }
        /// <summary>A bejelentkezett felhasználó azonosítója, vagy null, ha még nincs (pl. frissítés után) vagy érvénytelen.</summary>
        public Guid? UserGuid => Guid.TryParse(_userId, out var id) ? id : null;
        public string? UserName { get; set; } = string.Empty;
        private string _title = "Kezdőlap";

        /// <summary>A fejlécben megjelenő oldalcím; változáskor a <see cref="TitleChanged"/> esemény jelez a layoutnak.</summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value;
                TitleChanged?.Invoke();
            }
        }

        public event Action? TitleChanged;
        /// <summary>
        /// Az összeállítás alatt álló teszt kérdés-kosara (QuestionView → NewTest/NewTestWizard). Félkész állapot,
        /// ezért nem tölthető vissza az API-ból. A tesztet és az eredményeket az oldalak már azonosító alapján töltik
        /// (TestLoader); a korábbi Test és TestResults tulajdonság megszűnt.
        /// </summary>
        public List<TestQuestion> Questions { get; set; } = new();
        //public event Action Userchangedevent;
    }
}