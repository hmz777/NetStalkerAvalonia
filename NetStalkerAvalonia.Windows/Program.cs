using Avalonia;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Helpers;
using Serilog;
using System;

namespace NetStalkerAvalonia.Windows
{
	public class Program
	{
		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
				BuildAvaloniaApp()
					.StartWithClassicDesktopLifetime(args);
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), nameof(Main), e.Message);
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
		{
			// Read app config
			Config.ReadConfiguration();

			DependencyInjectionHelpers.RegisterAppDependencies();

			// Register platform specific deps
			Helpers.DependencyInjectionHelpers.RegisterAppDependencies();

			return AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.LogToTrace()
				.UseReactiveUI();
		}
	}
}