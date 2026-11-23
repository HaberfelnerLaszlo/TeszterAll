using Bunit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Components.MathLesson;
using TeszterLibrary.ViewModel;

namespace TeszterTests.Components
{
	/// <summary>A haladás-panel jelvényei: megszerzett, még nincs meg, és megerősítésre váró (link a megerősítő oldalra).</summary>
	public class StudentProgressTests : BunitContext
	{
		public StudentProgressTests()
		{
			JSInterop.Mode = JSRuntimeMode.Loose;
			Services.AddFluentUIComponents();
		}

		[Fact]
		public void Pending_badge_links_to_the_confirmation_page_with_friendly_text()
		{
			var progress = new StudentProgressView
			{
				Level = 1,
				NextLevelXp = 100,
				Badges =
				[
					new BadgeView { Code = "elso-lecke", Name = "Első lépés", Earned = true },
					new BadgeView { Code = "villam", Name = "Villám", Pending = true },
					new BadgeView { Code = "szazas", Name = "Százas" },
				],
			};

			var cut = Render<StudentProgress>(p => p.Add(c => c.Progress, progress));

			var pending = cut.Find("a.badgeChip.pending");
			Assert.Equal("/student/badge/villam", pending.GetAttribute("href"));
			Assert.Contains("majdnem a tiéd", pending.TextContent);
			Assert.DoesNotContain("csal", cut.Markup, StringComparison.OrdinalIgnoreCase);
			Assert.Single(cut.FindAll("span.badgeChip.earned"));
			Assert.Equal(2, cut.FindAll("span.badgeChip").Count);
		}
	}
}
