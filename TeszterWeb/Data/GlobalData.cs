using TeszterLibrary.Model;

using TeszterWeb.Components.Pages.Teacher;

namespace TeszterWeb.Data
{
    public class GlobalData
    {
        private string _userId = string.Empty;

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                //Userchangedevent?.Invoke();
            }
        }

        public List<Question> Questions { get; set; } = new List<Question>();
        public Test Test { get; set; } = new Test();
        public List<TeszterLibrary.ViewModel.QuestionView> TestResults { get; set; } = new();
        //public event Action Userchangedevent;
    }
}