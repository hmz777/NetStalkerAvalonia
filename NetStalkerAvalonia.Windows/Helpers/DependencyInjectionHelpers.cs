using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Windows.Services.Implementations;
using Splat;

namespace NetStalkerAvalonia.Windows.Helpers
{
	public class DependencyInjectionHelpers
	{
		public static void RegisterAppDependencies()
		{
			RegisterRequiredServices();
		}

		private static void RegisterRequiredServices()
		{
			// Source generation not working from this assembly, we'll register via the locator for now
			//SplatRegistrations.RegisterLazySingleton<IAppLockService, AppLockManagerWindows>();
			Locator.CurrentMutable.RegisterLazySingleton(() => new AppLockManagerWindows(), typeof(IAppLockService));
		}
	}
}