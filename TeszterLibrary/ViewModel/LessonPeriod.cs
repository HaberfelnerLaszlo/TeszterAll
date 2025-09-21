using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.ViewModel
{
    public class LessonPeriod
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<LessonPeriod> GetLessonPeriod()
        {
            return new()
            {
                new LessonPeriod { Id = 1, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 45, 0) },
                new LessonPeriod { Id = 2, StartTime = new TimeSpan(8, 55, 0), EndTime = new TimeSpan(9, 40, 0) },
                new LessonPeriod { Id = 3, StartTime = new TimeSpan(9, 55, 0), EndTime = new TimeSpan(10, 40, 0) },
                new LessonPeriod { Id = 4, StartTime = new TimeSpan(10, 50, 0), EndTime = new TimeSpan(11, 35, 0) },
                new LessonPeriod { Id = 5, StartTime = new TimeSpan(11, 45, 0), EndTime = new TimeSpan(12, 30, 0) },
                new LessonPeriod { Id = 6, StartTime = new TimeSpan(12, 40, 0), EndTime = new TimeSpan(13, 25, 0) },
                new LessonPeriod { Id = 7, StartTime = new TimeSpan(13, 35, 0), EndTime = new TimeSpan(14, 20, 0) },
            };
        }
    }
}
