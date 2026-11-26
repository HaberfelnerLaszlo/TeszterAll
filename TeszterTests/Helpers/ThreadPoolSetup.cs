using System.Runtime.CompilerServices;

namespace TeszterTests.Helpers
{
	/// <summary>
	/// A szálkészlet minimumának emelése a tesztszerelvény betöltésekor (a bUnit ajánlása szerint).
	/// <para>
	/// Az xUnit a tesztosztályokat párhuzamosan futtatja; a bUnit-tesztek renderelője és a WaitFor*-ellenőrzések is a
	/// szálkészleten futnak. Kevés magos CI-gépen a szálkészlet lassan bővül, és egy-egy ellenőrzés a teljes várakozási
	/// idő alatt sem kapott szálat („The wait condition was never checked … the thread pool may be starved”), így a teszt
	/// véletlenszerűen elbukott, bár a komponens jól működött.
	/// </para>
	/// </summary>
	internal static class ThreadPoolSetup
	{
		[ModuleInitializer]
		internal static void RaiseMinThreads()
		{
			ThreadPool.GetMinThreads(out var workers, out var io);
			ThreadPool.SetMinThreads(Math.Max(workers, Environment.ProcessorCount * 4), io);
		}
	}
}
