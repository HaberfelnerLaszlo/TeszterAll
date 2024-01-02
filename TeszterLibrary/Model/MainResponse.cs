using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.Model
{
    public class MainResponse
    {
        public bool IsSucccess { get; set; }
        public string ErrorMassage { get; set; } = string.Empty;
        public object? Content { get; set; }
        public void Clear()
        {
            IsSucccess = false;
            ErrorMassage = string.Empty;
            Content = null;
        }
    }
}
