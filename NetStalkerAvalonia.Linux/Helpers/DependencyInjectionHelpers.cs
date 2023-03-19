using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Linux.Services.Implementations;
using Splat;
using System.IO.Abstractions;

namespace NetStalkerAvalonia.Linux.Helpers
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
			Locator.CurrentMutable.RegisterLazySingleton(() => new AppLockManagerLinux(Locator.Current.GetService<IFileSystem>()), typeof(IAppLockService));
		}
	}
}