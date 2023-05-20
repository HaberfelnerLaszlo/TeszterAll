using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;


namespace TeszterLibrary.Model
{
    public class Node
    {
        public int ID;
        public Vector2 Position { get; set; } = new Vector2 { X = 10, Y = 10 };
        public string Text { get; set; }=string.Empty;
        public string Color=string.Empty;
        public int Number { get; set; }
        public int PairNumber { get; set; }
        public int PairId { get; set; } = -1;
        public bool IsPair { get; set; } = false;
    }
}
