using Bunit;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Components.MathLesson;
using TeszterLibrary.ViewModel;

namespace TeszterTests.Components
{
	/// <summary>
	/// A feladat-beviteli mező: Enterre a beírt válasz megy el. Régen a mező csak change-re frissült, így az Enter üres
	/// választ küldött („Ne hagyd üresen…”), az űrlap natív elküldése pedig újratöltötte az oldalt (új feladat).
	/// </summary>
	public class MathLessonTests : BunitContext
	{
		public MathLessonTests()
		{
			JSInterop.Mode = JSRuntimeMode.Loose;
			Services.AddFluentUIComponents();
		}

		private IRenderedComponent<MathLesson> Render(OperationView op, List<OperationView> sent, bool fraction = false) =>
			Render<MathLesson>(p => p
				.Add(c => c.Assignment, op)
				.Add(c => c.IsFraction, fraction)
				.Add(c => c.Send, (OperationView o) => sent.Add(new OperationView { OperationId = o.OperationId, Result = o.Result })));

		[Fact]
		public void Enter_sends_the_typed_answer()
		{
			var sent = new List<OperationView>();
			var cut = Render(new OperationView { OperationId = 7, Sequence = "3 + 4 Ł = Ł" }, sent);

			var input = cut.Find("input.valasz");
			input.Input("7");                                              // minden leütésre frissül (oninput)
			input.KeyDown(new KeyboardEventArgs { Code = "Enter", Key = "Enter" });

			var answer = Assert.Single(sent);
			Assert.Equal("7", answer.Result);
			Assert.DoesNotContain("Ne hagyd üresen", cut.Markup);
		}

		[Fact]
		public void Numpad_enter_sends_too_and_an_empty_answer_is_not_sent()
		{
			var sent = new List<OperationView>();
			var cut = Render(new OperationView { OperationId = 8, Sequence = "5 + 5 Ł = Ł" }, sent);

			var input = cut.Find("input.valasz");
			input.KeyDown(new KeyboardEventArgs { Code = "NumpadEnter", Key = "Enter" });
			Assert.Empty(sent);
			Assert.Contains("Ne hagyd üresen", cut.Markup);

			input.Input("10");
			input.KeyDown(new KeyboardEventArgs { Code = "NumpadEnter", Key = "Enter" });
			Assert.Equal("10", Assert.Single(sent).Result);
		}

		[Fact]
		public void Fraction_fields_update_on_input_and_enter_sends_them()
		{
			var sent = new List<OperationView>();
			var cut = Render(new OperationView { OperationId = 9, Sequence = "1/2 + 1/4 Ł = Ł" }, sent, fraction: true);

			var fields = cut.FindAll("input.valasz");
			fields[0].Input("3");
			cut.FindAll("input.valasz")[1].Input("4");
			cut.FindAll("input.valasz")[1].KeyDown(new KeyboardEventArgs { Code = "Enter", Key = "Enter" });

			Assert.Equal("3/4", Assert.Single(sent).Result);
		}

		[Fact]
		public void The_form_never_submits_natively()
		{
			var cut = Render(new OperationView { OperationId = 10, Sequence = "2 + 2 Ł = Ł" }, []);
			// A natív elküldés újratöltené az oldalt (és új feladatot kérne); a Blazor ezt preventDefault-tal tiltja.
			var form = cut.Find("form");
			Assert.True(form.HasAttribute("blazor:onsubmit:preventDefault"));
		}
	}
}
