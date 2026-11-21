using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;

using TeszterLibrary.Extensions;
using TeszterLibrary.Model;
using TeszterLibrary.Services;
using TeszterLibrary.ViewModel;

namespace TeszterWeb.Services
{
    /// <summary>Egy betöltés eredménye: az érték, vagy a felhasználónak szóló hibaüzenet.</summary>
    public sealed record Loaded<T>(T? Value, string? Error) where T : class
    {
        public bool Ok => Error is null && Value is not null;
        public static Loaded<T> Success(T value) => new(value, null);
        public static Loaded<T> Fail(string error) => new(null, error);
    }

    /// <summary>
    /// A tesztoldalak az URL-ben kapott azonosító alapján maguk töltik be az adataikat (régen a GlobalData-n át kapták
    /// az előző oldaltól, így frissítés (F5) után üresek lettek: eltűnt az időzítő és az eredmény, szerkesztéskor pedig
    /// egy üres teszt nyílt meg). A diák csak a saját tesztjét nyithatja meg: a bejelentkezett felhasználót az Entra
    /// token oid claimjéből vesszük (ez frissítés után is megvan), és csak a saját listáiban szereplő TestUser-t engedjük.
    /// Ez nem váltja ki az API-oldali ellenőrzést.
    /// </summary>
    public sealed class TestLoader(
        TestServices tests,
        TestUserServices testUsers,
        TestQuestionServices testQuestions,
        AuthenticationStateProvider authentication)
    {
        public const string NotYours = "Ez a teszt nem érhető el: nem a tiéd, vagy már lezárult.";

        /// <summary>A bejelentkezett felhasználó Entra objektumazonosítója (= Graph user.Id), vagy null.</summary>
        public async Task<Guid?> CurrentUserIdAsync()
        {
            var state = await authentication.GetAuthenticationStateAsync();
            return state.User.GetObjectId().ToGuidOrNull();
        }

        /// <summary>A diák egy még nem kész tesztjének beállításai (időzítő, ellenőrzés, keverés...) a TestUser azonosító alapján.</summary>
        public async Task<Loaded<Test>> LoadTestForStudentAsync(int testUserId)
        {
            if (testUserId <= 0) return Loaded<Test>.Fail("Érvénytelen teszt azonosító.");
            if (await CurrentUserIdAsync() is not Guid me) return Loaded<Test>.Fail("Nem sikerült azonosítani a felhasználót. Jelentkezz be újra!");

            var mine = await testUsers.GetTests(me);
            if (mine is null) return Loaded<Test>.Fail(Message(testUsers.ErrorMessage, "Nem sikerült lekérni a tesztjeidet."));

            var view = mine.FirstOrDefault(t => t.TestUserId == testUserId);
            return view is null ? Loaded<Test>.Fail(NotYours) : Loaded<Test>.Success(ToTest(view));
        }

        /// <summary>
        /// A Teams-feladat linkje (/start/{tesztId}) alapján a diák saját, még nem kész tesztje (benne a TestUserId).
        /// Régen a Startpage a teljes tesztet kérte le (GET Test/{id}, a többi diák azonosítójával együtt); ez a végpont
        /// mostantól csak tanárnak érhető el, a diák a saját listájából dolgozik (TUser/Tests/{oid}, tulajdonos-ellenőrzött).
        /// </summary>
        public async Task<Loaded<TestView>> FindStudentTestAsync(int testId, DateTime today)
        {
            if (testId <= 0) return Loaded<TestView>.Fail($"Nincs ilyen teszt! Azonosító: {testId}");
            if (await CurrentUserIdAsync() is not Guid me) return Loaded<TestView>.Fail("Nem sikerült azonosítani a felhasználót. Jelentkezz be újra!");

            var mine = await testUsers.GetTests(me);
            var view = mine?.FirstOrDefault(t => t.Id == testId);
            if (view is null) return Loaded<TestView>.Fail("Ez a teszt nem érhető el: nem neked adták ki, vagy már megírtad.");
            if (view.EndTime < today) return Loaded<TestView>.Fail("A teszt határideje lejárt.");
            return Loaded<TestView>.Success(view);
        }

        /// <summary>A diák egy tesztjének kérdésenkénti eredménye, csak a saját (kész vagy folyamatban lévő) tesztjére.</summary>
        public async Task<Loaded<List<QuestionView>>> LoadResultsForStudentAsync(int testUserId)
        {
            if (testUserId <= 0) return Loaded<List<QuestionView>>.Fail("Érvénytelen teszt azonosító.");
            if (await CurrentUserIdAsync() is not Guid me) return Loaded<List<QuestionView>>.Fail("Nem sikerült azonosítani a felhasználót. Jelentkezz be újra!");

            var ready = await testUsers.GetReadyTests(me) ?? [];
            var owned = ready.Any(t => t.TestUserId == testUserId)
                || (await testUsers.GetTests(me) ?? []).Any(t => t.TestUserId == testUserId);
            if (!owned) return Loaded<List<QuestionView>>.Fail(NotYours);

            var questions = await testUsers.GetTestQuestions(testUserId);
            return questions is null
                ? Loaded<List<QuestionView>>.Fail(Message(testUsers.ErrorMessage, "Nem sikerült lekérni az eredményeidet."))
                : Loaded<List<QuestionView>>.Success(questions);
        }

        /// <summary>
        /// Egy teszt a tanári oldalaknak (eredmények, közzététel, szerkesztés). A kérdéseket és a tesztírókat külön
        /// végpontról töltjük, ha a teszt nem tartalmazza őket.
        /// </summary>
        public async Task<Loaded<Test>> LoadTestForTeacherAsync(int testId, bool withQuestions = false, bool withUsers = false)
        {
            if (testId <= 0) return Loaded<Test>.Fail("Érvénytelen teszt azonosító.");

            var test = await tests.GetTest(testId);
            if (test is null) return Loaded<Test>.Fail("A teszt nem található.");

            if (withQuestions && (test.TestQuestions is null || test.TestQuestions.Count == 0))
            {
                var questions = await testQuestions.GetTestQuestions(testId);
                if (questions is null) return Loaded<Test>.Fail(Message(testQuestions.ErrorMessage, "Nem sikerült lekérni a teszt kérdéseit."));
                test.TestQuestions = questions;
            }
            if (withUsers && (test.TestUsers is null || test.TestUsers.Count == 0))
            {
                test.TestUsers = await testUsers.GetTestUsers(testId) ?? [];
            }
            return Loaded<Test>.Success(test);
        }

        private static Test ToTest(TestView view) => new()
        {
            Id = view.Id,
            Title = view.Title,
            Time = view.Time,
            IsCheck = view.IsCheck,
            IsPractice = view.IsPractice,
            IsTimer = view.IsTimer,
            ShuffleQuestion = view.ShuffleQuestion,
            Subject = view.Subject,
            EndTime = view.EndTime,
        };

        private static string Message(string? error, string fallback) => string.IsNullOrWhiteSpace(error) ? fallback : error;
    }
}
