using System.Diagnostics;

using Bunit;

using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TeszterLibrary.Components;

namespace TeszterTests.Components
{
    /// <summary>A visszaszámláló: valós idejű határidő, pontosan egyszeri IsEnd, tiszta leállás.</summary>
    public class CountDownTests : BunitContext
    {
        public CountDownTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddFluentUIComponents();
        }

        private IRenderedComponent<CountDown> Render(TimeSpan remaining, Action<bool> onEnd)
            => Render<CountDown>(p => p.Add(c => c._remainingTime, remaining).Add(c => c.IsEnd, onEnd));

        [Fact]
        public async Task IsEnd_fires_once_at_the_deadline_and_a_parent_rerender_does_not_reset_the_clock()
        {
            var ended = 0;
            var value = false;
            var stopwatch = Stopwatch.StartNew();
            var cut = Render(TimeSpan.FromSeconds(3), b => { Interlocked.Increment(ref ended); value = b; });

            await Task.Delay(1500);
            cut.Render(p => p.Add(c => c._remainingTime, TimeSpan.FromSeconds(3)));   // a szülő újrarenderel: ugyanaz az érték
            await cut.WaitForStateAsync(() => Volatile.Read(ref ended) > 0, TimeSpan.FromSeconds(8));
            var seconds = stopwatch.Elapsed.TotalSeconds;
            await Task.Delay(2500);                                                                    // ne hívja meg másodszor

            Assert.Equal(1, ended);
            Assert.True(value);
            Assert.InRange(seconds, 2.7, 3.7);          // régen a szülő újrarenderelése nullázta az órát, ez ~4,5 mp lett volna
        }

        [Fact]
        public async Task Disposing_before_the_deadline_never_calls_IsEnd()
        {
            var ended = 0;
            Render(TimeSpan.FromSeconds(2), _ => Interlocked.Increment(ref ended));
            await Task.Delay(600);

            await DisposeComponentsAsync();
            await Task.Delay(3000);

            Assert.Equal(0, ended);
        }

        [Fact]
        public async Task A_changed_initial_value_restarts_the_countdown()
        {
            var ended = 0;
            var cut = Render(TimeSpan.FromSeconds(1), _ => Interlocked.Increment(ref ended));
            await cut.WaitForStateAsync(() => Volatile.Read(ref ended) == 1, TimeSpan.FromSeconds(4));

            cut.Render(p => p.Add(c => c._remainingTime, TimeSpan.FromSeconds(2)));

            await cut.WaitForStateAsync(() => Volatile.Read(ref ended) == 2, TimeSpan.FromSeconds(5));
        }
    }
}
