using System.Runtime.CompilerServices;

namespace AudibleApi.Tests;

internal class ModuleInitializer
{
	[ModuleInitializer]
	internal static void Init()
	{
		// Force enumeration of ResultFactory to avoid race condition with parallelized tests
		_ = ResultFactory.GetAll().ToArray();
	}
}
