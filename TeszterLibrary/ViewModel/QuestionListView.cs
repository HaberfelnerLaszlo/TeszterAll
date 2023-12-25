using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeszterLibrary.Model;

namespace TeszterLibrary.ViewModel
{
    internal class QuestionListView:Question
    {
        public bool SelectedTest { get; set; }
    }
}
