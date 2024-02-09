using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.Extensions
{
    public static class IntExtension
    {
        public static string ToTime(this int value)
        {
            if (value < 0) { return string.Empty; }
            if (value> 3600)
            {
                return $"{value / 3600} ó : {(value % 3600) / 60} p : {(value % 3600) % 60} mp";
            }
            if (value < 60) { return value.ToString() + " mp"; }
            else { return $"{value / 60} p : {value % 60} mp"; }
        }

    }
}
