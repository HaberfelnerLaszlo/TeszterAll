using System;
using System.Collections.Generic;
using System.Text;

namespace TeszterLibrary.Events
{
    public class MessageEvent : EventArgs
    {
        public string Message { get; set; }  = string.Empty;
        public int Code { get; set; } = 0;

        public MessageEvent(string message, int code = 0)
        {
            Message = message;
            Code = code;
        }
    }
}
