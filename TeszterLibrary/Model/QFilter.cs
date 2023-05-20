using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.Model
{
    public class QFilter
    {
        public string Subject { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SearchText { get; set; }= string.Empty;
        public QuestionType QType { get; set; } = QuestionType.All;
        public int MinGrade { get; set; } = 1;
        public int MaxGrade { get; set; } = 12;
    }
}
