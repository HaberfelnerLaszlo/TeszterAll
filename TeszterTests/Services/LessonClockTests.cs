using TeszterLibrary.Services;
using TeszterLibrary.ViewModel;

namespace TeszterTests.Services
{
    /// <summary>
    /// A tanóra-időbeosztás (órai nézet): mikor jelenik meg a feladat, és meddig kell várni.
    /// Csengetési rend: 8:00–8:45, 8:55–9:40, 9:55–10:40, 10:50–11:35, 11:45–12:30, 12:40–13:25, 13:35–14:20.
    /// </summary>
    public class LessonClockTests
    {
        private static readonly DateTime Monday = new(2026, 11, 9);

        private static DateTime At(int hour, int minute, int second = 0, int dayOffset = 0)
            => Monday.AddDays(dayOffset).AddHours(hour).AddMinutes(minute).AddSeconds(second);

        private static LessonWindow Eval(int hour, int minute, int second = 0, int dayOffset = 0)
            => LessonClock.Evaluate(At(hour, minute, second, dayOffset));

        private sealed class FixedTime(DateTimeOffset utc) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => utc;
        }

        [Fact]
        public void The_test_calendar_starts_on_a_monday()
        {
            Assert.Equal(DayOfWeek.Monday, Monday.DayOfWeek);
        }

        [Theory]
        [InlineData(5, 3, 0)]     // szombat éjjel
        [InlineData(5, 23, 59)]   // szombat késő este: nem "óra utáni", hanem hétvége
        [InlineData(6, 10, 0)]    // vasárnap tanítási időben
        public void Weekends_are_always_lesson_time(int dayOffset, int hour, int minute)
        {
            var window = Eval(hour, minute, dayOffset: dayOffset);

            Assert.True(window.IsLessonTime);
            Assert.Equal(LessonClock.WaitTime, window.Remaining);
            Assert.Equal(LessonReason.Weekend, window.Reason);
        }

        [Fact]
        public void Before_the_first_lesson_the_page_waits_until_it_starts()
        {
            var window = Eval(7, 30);

            Assert.False(window.IsLessonTime);
            Assert.Equal(TimeSpan.FromMinutes(30), window.Remaining);
            Assert.Equal(LessonReason.BeforeFirstLesson, window.Reason);
            Assert.Equal(TimeSpan.FromHours(8), Eval(0, 0).Remaining);
        }

        [Theory]
        [InlineData(8, 0)]     // a kezdés már óra
        [InlineData(8, 45)]    // a vége is
        [InlineData(10, 20)]
        [InlineData(14, 20)]   // az utolsó óra vége
        public void During_a_lesson_including_its_start_and_end_the_task_is_shown(int hour, int minute)
        {
            var window = Eval(hour, minute);

            Assert.True(window.IsLessonTime);
            Assert.Equal(LessonReason.DuringLesson, window.Reason);
            Assert.Equal(LessonClock.WaitTime, window.Remaining);
        }

        [Theory]
        [InlineData(8, 46, 9)]     // 8:55-ig 9 perc
        [InlineData(9, 41, 14)]    // 9:55-ig 14 perc
        [InlineData(9, 54, 1)]
        [InlineData(13, 26, 9)]    // 13:35-ig
        public void Between_lessons_the_page_waits_for_the_next_start(int hour, int minute, int expectedMinutes)
        {
            var window = Eval(hour, minute);

            Assert.False(window.IsLessonTime);
            Assert.Equal(TimeSpan.FromMinutes(expectedMinutes), window.Remaining);
            Assert.Equal(LessonReason.BetweenLessons, window.Reason);
        }

        [Fact]
        public void Seconds_are_ignored_like_in_the_original_implementation()
        {
            Assert.Equal(LessonReason.DuringLesson, Eval(8, 45, 59).Reason);     // még 8:45
            Assert.Equal(LessonReason.BetweenLessons, Eval(8, 46, 0).Reason);
        }

        [Theory]
        [InlineData(14, 21)]
        [InlineData(18, 0)]
        [InlineData(23, 59)]
        public void After_the_last_lesson_free_practice_is_allowed(int hour, int minute)
        {
            var window = Eval(hour, minute);

            Assert.True(window.IsLessonTime);
            Assert.Equal(LessonReason.AfterLastLesson, window.Reason);
            Assert.Equal(LessonClock.WaitTime, window.Remaining);
        }

        [Fact]
        public void Without_configured_periods_nobody_has_to_wait()
        {
            var window = LessonClock.Evaluate(At(10, 0), Array.Empty<LessonPeriod>());

            Assert.True(window.IsLessonTime);
            Assert.Equal(TimeSpan.Zero, window.Remaining);
            Assert.Equal(LessonReason.NoPeriodsConfigured, window.Reason);
        }

        [Fact]
        public void The_next_lesson_is_found_by_position_not_by_id()
        {
            // a régi kód Periods[item.Id]-t indexelt, vagyis az Id-ra támaszkodott
            var periods = new List<LessonPeriod>
            {
                new() { Id = 10, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 45, 0) },
                new() { Id = 99, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(9, 45, 0) },
            };

            var window = LessonClock.Evaluate(At(8, 50), periods);

            Assert.False(window.IsLessonTime);
            Assert.Equal(TimeSpan.FromMinutes(10), window.Remaining);
        }

        [Fact]
        public void Every_minute_of_a_school_day_is_consistent_and_never_throws()
        {
            var periods = new LessonPeriod().GetLessonPeriod();
            for (var minute = 0; minute < 24 * 60; minute++)
            {
                var now = new TimeSpan(0, minute, 0);
                var window = LessonClock.Evaluate(Monday.Add(now), periods);

                if (window.IsLessonTime)
                {
                    Assert.Equal(LessonClock.WaitTime, window.Remaining);
                }
                else
                {
                    Assert.True(window.Remaining > TimeSpan.Zero, $"{now}: a várakozás nem lehet 0");
                    Assert.True(periods.Any(p => p.StartTime == now + window.Remaining), $"{now}: a várakozás nem egy óra kezdetére mutat");
                }
                if (window.Reason == LessonReason.DuringLesson)
                {
                    Assert.True(periods.Any(p => p.StartTime <= now && p.EndTime >= now), $"{now}: nincs ilyen óra");
                }
            }
        }

        // ---------------------------------------------------------------- iskolai idő (időzóna)

        [Theory]
        [InlineData("2026-01-15T12:00:00Z", 13)]   // tél: UTC+1
        [InlineData("2026-07-15T12:00:00Z", 14)]   // nyár: UTC+2
        [InlineData("2026-03-29T00:59:00Z", 1)]    // az óraátállítás előtt (CET)
        [InlineData("2026-03-29T01:00:00Z", 3)]    // az átállítás után (CEST)
        [InlineData("2026-10-25T00:59:00Z", 2)]    // az őszi átállítás előtt (CEST)
        [InlineData("2026-10-25T01:00:00Z", 2)]    // utána (CET)
        public void School_time_follows_central_european_time_including_daylight_saving(string utc, int expectedHour)
        {
            var school = LessonClock.SchoolNow(new FixedTime(DateTimeOffset.Parse(utc)));

            Assert.Equal(expectedHour, school.Hour);
        }

        [Fact]
        public void A_server_clock_in_utc_still_yields_the_right_lesson()
        {
            // hétfő 09:30 CET = 08:30 UTC: a 2. óra közepe
            var window = LessonClock.Evaluate(LessonClock.SchoolNow(new FixedTime(DateTimeOffset.Parse("2026-11-09T08:30:00Z"))));

            Assert.Equal(LessonReason.DuringLesson, window.Reason);
        }

        [Fact]
        public void The_weekend_is_decided_by_school_time_not_by_the_server_clock()
        {
            // péntek 23:30 UTC már szombat 00:30 CET: a régi kód (DateTime.Now) UTC-s szerveren még péntekként kezelte
            var school = LessonClock.SchoolNow(new FixedTime(DateTimeOffset.Parse("2026-11-13T23:30:00Z")));

            Assert.Equal(DayOfWeek.Saturday, school.DayOfWeek);
            Assert.Equal(LessonReason.Weekend, LessonClock.Evaluate(school).Reason);
        }
    }
}
