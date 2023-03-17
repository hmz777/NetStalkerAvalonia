using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using NetStalkerAvalonia.Helpers;

namespace NetStalkerAvalonia.Core.Helpers
{
	public class LifeTimeHelpers
	{
		public static void ShowApp()
		{
			// Temporary fix
			// TODO: Find a more stable fix

			StaticData.MainWindow!.WindowState = Avalonia.Controls.WindowState.Normal;
			StaticData.MainWindow!.WindowState = Avalonia.Controls.WindowState.Minimized;
			StaticData.MainWindow!.WindowState = Avalonia.Controls.WindowState.Normal;
		}

		public static void ExitApp()
		{
			var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
			app?.Shutdown();
		}
	}
}
