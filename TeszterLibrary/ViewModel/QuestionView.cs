using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeszterLibrary.Model;

namespace TeszterLibrary.ViewModel
{
    public class QuestionView
    {
        /// <summary>
        /// A teszt kérdés azonosítója
        /// </summary>
        public int TestQuestionId { get; set; }
        /// <summary>
        /// Kérdés szövege.
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;
        /// <summary>
        /// A kérdés típusa.
        /// </summary>
        public QuestionType QuestionType { get; set; }
        /// <summary>
        /// Kérdésre kapható pontok. Vagy viszza fele a kérdésre kapott pontok.
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// A kérdésre fordítható idő
        /// </summary>
        public int Time { get; set; }
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
