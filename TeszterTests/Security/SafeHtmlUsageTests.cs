using System.Text.RegularExpressions;

namespace TeszterTests.Security
{
    /// <summary>
    /// Őrteszt: a .razor fájlokban nem lehet közvetlen MarkupString-konverzió (csak szöveg-literálra, pl. emoji),
    /// minden HTML-megjelenítés a SafeHtml.Markup-on át megy. Új hely esetén a teszt elbukik.
    /// </summary>
    public class SafeHtmlUsageTests
    {
        private static readonly Regex RazorComment = new(@"@\*.*?\*@", RegexOptions.Singleline);
        private static readonly Regex RawMarkup = new(@"\(\s*MarkupString\s*\)\s*(?!""[^""]*""\s*\))|new\s+MarkupString\s*\(");

        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TeszterAll.sln")))
            {
                dir = dir.Parent;
            }
            return dir?.FullName ?? throw new InvalidOperationException("Nem található a TeszterAll.sln.");
        }

        public static TheoryData<string> Projects => new() { "TeszterLibrary", "TeszterWeb" };

        [Theory]
        [MemberData(nameof(Projects))]
        public void Razor_files_render_html_only_through_SafeHtml(string project)
        {
            var root = Path.Combine(RepoRoot(), project);
            var files = Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                         && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToList();
            Assert.NotEmpty(files);

            var violations = files
                .SelectMany(f =>
                {
                    var text = RazorComment.Replace(File.ReadAllText(f), m => new string('\n', m.Value.Count(c => c == '\n')));
                    return text.Split('\n')
                        .Select((line, i) => (line, i))
                        .Where(x => RawMarkup.IsMatch(x.line))
                        .Select(x => $"{Path.GetRelativePath(root, f)}:{x.i + 1}: {x.line.Trim()}");
                })
                .ToList();

            Assert.True(violations.Count == 0,
                "Közvetlen MarkupString-konverzió (használd a SafeHtml.Markup-ot):" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Theory]
        [InlineData("@((MarkupString)question.Text)", true)]
        [InlineData("@( (MarkupString)a.Last)", true)]
        [InlineData("@((new MarkupString(item.Text)))", true)]
        [InlineData("@((MarkupString)\"\\U0001F518\")", false)]
        [InlineData("@SafeHtml.Markup(question.Text)", false)]
        public void The_guard_pattern_recognises_raw_conversions(string line, bool flagged)
        {
            Assert.Equal(flagged, RawMarkup.IsMatch(line));
        }
    }
}
