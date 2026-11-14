using TeszterLibrary.ViewModel;

namespace TeszterLibrary.Services
{
    public enum LessonReason
    {
        /// <summary>Hétvége: mindig "gyakorló" idő.</summary>
        Weekend,
        /// <summary>Nincs beállított tanóra: nem várakoztatunk.</summary>
        NoPeriodsConfigured,
        /// <summary>Az első tanóra előtt: várni kell.</summary>
        BeforeFirstLesson,
        /// <summary>Óra van (a kezdés és a vége is beleszámít).</summary>
        DuringLesson,
        /// <summary>Szünet két óra között: várni kell a következő kezdésig.</summary>
        BetweenLessons,
        /// <summary>Az utolsó óra után: szabad gyakorlás.</summary>
        AfterLastLesson,
    }

    /// <param name="IsLessonTime">Igaz: a feladat megjeleníthető (óra vagy gyakorló idő van).</param>
    /// <param name="Remaining">Várakozásnál a következő óráig hátralévő idő, egyébként a megjelenített feladat látható ideje.</param>
    public sealed record LessonWindow(bool IsLessonTime, TimeSpan Remaining, LessonReason Reason);

    /// <summary>
    /// A tanóra-időbeosztás kiértékelése: mikor jeleníthető meg az órai feladat, és meddig kell várni.
    /// Korábban az Index.razor-ban volt; innen a tanári és a névtelen (kivetítős) nézet is használja, és tesztelhető.
    /// </summary>
    public static class LessonClock
    {
        /// <summary>A feladat láthatósági ideje a megjelenítés után (óra alatt, hétvégén, óra után).</summary>
        public static readonly TimeSpan WaitTime = TimeSpan.FromMinutes(6);

        private static readonly Lazy<TimeZoneInfo> SchoolTimeZone = new(ResolveTimeZone);

        /// <summary>Az iskolai (közép-európai) idő. A TimeProvider miatt tesztben bármely időpont szimulálható.</summary>
        public static DateTime SchoolNow(TimeProvider? timeProvider = null)
        {
            var utc = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;
            return TimeZoneInfo.ConvertTimeFromUtc(utc, SchoolTimeZone.Value);
        }

        /// <param name="schoolNow">Iskolai idő (lásd <see cref="SchoolNow"/>); a hét napja és az óra:perc számít, a másodperc nem.</param>
        /// <param name="periods">Tanórák; alapértelmezés: a LessonPeriod beépített csengetési rendje.</param>
        public static LessonWindow Evaluate(DateTime schoolNow, IReadOnlyList<LessonPeriod>? periods = null)
        {
            periods ??= new LessonPeriod().GetLessonPeriod();

            if (schoolNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return new LessonWindow(true, WaitTime, LessonReason.Weekend);

            if (periods.Count == 0)
                return new LessonWindow(true, TimeSpan.Zero, LessonReason.NoPeriodsConfigured);

            var now = new TimeSpan(schoolNow.Hour, schoolNow.Minute, 0);

            if (periods[0].StartTime - now > TimeSpan.Zero)
                return new LessonWindow(false, periods[0].StartTime - now, LessonReason.BeforeFirstLesson);

            if (periods[^1].EndTime - now < TimeSpan.Zero)
                return new LessonWindow(true, WaitTime, LessonReason.AfterLastLesson);

            for (var i = 0; i < periods.Count; i++)
            {
                if (periods[i].StartTime <= now && periods[i].EndTime >= now)
                    return new LessonWindow(true, WaitTime, LessonReason.DuringLesson);

                // a következő óra indexelése (nem az Id-ra támaszkodunk, így bármilyen azonosítóval működik)
                if (i + 1 < periods.Count && periods[i + 1].StartTime > now)
                    return new LessonWindow(false, periods[i + 1].StartTime - now, LessonReason.BetweenLessons);
            }
            return new LessonWindow(false, TimeSpan.Zero, LessonReason.BetweenLessons);
        }

        // Windows-azonosító (a régi kód ezt használta) vagy IANA: a Dockerben/Linuxon és Windowson is működjön.
        private static TimeZoneInfo ResolveTimeZone()
        {
            foreach (var id in new[] { "Central European Standard Time", "Europe/Budapest" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Local;
        }
    }
}
