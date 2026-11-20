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
        public string Title { get; set; } = "Kezdőlap";
        public List<TestQuestion> Questions { get; set; } = new();
        public Test Test { get; set; } = new Test();
        public List<TeszterLibrary.ViewModel.QuestionView> TestResults { get; set; } = new();
        //public event Action Userchangedevent;
    }
}