using TeszterLibrary.Model;

namespace TeszterWeb.Data
{
    public class GlobalData
    {
        public string UserId { get; set; } = string.Empty;
        public List<Question> Questions { get; set; }=new List<Question>();
        public Test Test { get; set; }=new Test();

    }
}
