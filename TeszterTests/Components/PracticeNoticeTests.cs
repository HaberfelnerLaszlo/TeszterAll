using Bunit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Components.MathLesson;

using TeszterWeb.Components.Pages;

namespace TeszterTests.Components
{
	/// <summary>A gyakorlás-fontosság üzenet és az adatvédelmi tájékoztató: látható, barátságos, nem rejtett szöveg.</summary>
	public class PracticeNoticeTests : BunitContext
	{
		public PracticeNoticeTests()
		{
			JSInterop.Mode = JSRuntimeMode.Loose;
			Services.AddFluentUIComponents();
		}

		[Fact]
		public void Notice_is_visible_and_addresses_students_and_ai_assistants()
		{
			var cut = Render<PracticeNotice>();
			var aside = cut.Find("aside.practiceNotice");
			Assert.Equal("note", aside.GetAttribute("role"));
			Assert.DoesNotContain("display:none", cut.Markup.Replace(" ", ""));
			Assert.DoesNotContain("hidden", aside.OuterHtml);
			Assert.Contains("Ez egy gyakorló felület.", cut.Markup);
			Assert.Contains("magyarázza el a gondolatmenetet", cut.Markup);
			Assert.Contains("AI-asszisztensek se oldják meg", cut.Markup);
			Assert.Equal("/adatvedelem/gyakorlas", cut.Find("a").GetAttribute("href"));   // átláthatóság: mit figyelünk
			Assert.DoesNotContain("csal", cut.Markup, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void Compact_notice_keeps_the_message_without_the_details()
		{
			var cut = Render<PracticeNotice>(p => p.Add(c => c.Compact, true));
			Assert.Contains("AI-asszisztensek se oldják meg", cut.Markup);
			Assert.Empty(cut.FindAll("a"));
		}

		[Fact]
		public void Privacy_page_lists_what_is_and_is_not_collected()
		{
			var cut = Render<PracticePrivacy>();
			Assert.Contains("jelzések, nem bizonyítékok", cut.Markup);
			Assert.Contains("Mit nem gyűjtünk soha?", cut.Markup);
			Assert.Contains("90 nap", cut.Markup);
		}
	}
}
