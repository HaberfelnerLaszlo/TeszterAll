using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.ViewModel
{
    public class QuestionView
    {
        /// <summary>
        /// Kérdés szövege.
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;
        public List<AnswerView>? Answers { get; set; }
    }
    public record AnswerView
    {
        public int AnswerId { get; set; }
         /// <summary>
        /// A válasz
        /// </summary>
       public string Text { get; set; } = string.Empty;
        /// <summary>
        /// Megjelölt válasz.
        /// </summary>
        public bool Sign { get; set; } = false;
        /// <summary>
        /// Az adott válasz helyes volt-e.
        /// </summary>
        public bool Result { get; set; } = false;
        /// <summary>
        /// Sorbarendezésnél a sorrend vagy párosításnál a pár száma
        /// </summary>
        public int Sequence { get; set; }

    }
}
