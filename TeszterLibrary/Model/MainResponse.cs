using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.Model
{
    public class MainResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public object? Content { get; set; }
        public void Clear()
        {
            IsSuccess = false;
            ErrorMessage = string.Empty;
            Content = null;
        }
    }
}
