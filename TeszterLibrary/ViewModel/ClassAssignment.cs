using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.ViewModel
{
	internal class ClassAssignment
	{
		public string First;
		public string Result;
		public string Last;
		public bool IsCorrect;
		public ClassAssignment(string first, string r, string l, bool c)
		{
			First = first;
			Result = r;
			Last = l;
			IsCorrect = c;
		}
	}
}
