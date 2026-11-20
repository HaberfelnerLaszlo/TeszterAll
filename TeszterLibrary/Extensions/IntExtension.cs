using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.Extensions
{
    public static class IntExtension
    {
        /// <summary>
        /// Másodpercek olvasható alakban: "45 mp", "2 p : 05 mp", "1 ó : 00 p : 00 mp". Negatívra üres szöveg.
        /// (Régen 3600 mp "60 p : 0 mp" lett, mert a határ &gt; volt &gt;= helyett, és nem volt nullával kitöltés.)
        /// </summary>
        public static string ToTime(this int value)
        {
            if (value < 0) { return string.Empty; }
            if (value < 60) { return $"{value} mp"; }
            if (value < 3600) { return $"{value / 60} p : {(value % 60).PadLeft()} mp"; }
            return $"{value / 3600} ó : {(value % 3600 / 60).PadLeft()} p : {(value % 60).PadLeft()} mp";
        }
        public static string PadLeft(this int number) =>
    number.ToString().PadLeft(2, '0');
    }
}
