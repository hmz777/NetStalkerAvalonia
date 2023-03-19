using Avalonia;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Helpers;
using ReactiveUI;
using Serilog;
using Splat;
using System;
using System.Reflection;

namespace NetStalkerAvalonia.Core
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
			// Router uses Splat.Locator to resolve views for
			// view models, so we need to register our views.
			Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

			// Read app config
			ReadConfiguration();

			DependencyInjectionHelpers.RegisterAppDependencies();

			return AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.LogToTrace()
				.UseReactiveUI();
		}

		private static void ReadConfiguration()
		{
			Config.AppSettings.ReadConfiguration();
		}
	}
}