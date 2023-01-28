using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.Views;
using Serilog;
using System;
using System.Linq;

namespace NetStalkerAvalonia
{
	public partial class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			try
			{
				Tools.InitViewModels();

				if (StaticData.ViewModels.First() is not MainWindowViewModel mainViewModel)
				{
					throw new Exception("Error initializing view models!");
				}

				if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{
					DataContext = mainViewModel;

					desktop.MainWindow = new MainWindow
					{
						DataContext = mainViewModel
					};

					desktop.ShutdownRequested += (sender, args) =>
					{
						// Save device friendly names before exiting
						var deviceNameResolver = Tools.ResolveIfNull<IDeviceNameResolver>(null!);
						deviceNameResolver.SaveDeviceNamesAsync(mainViewModel.GetUiDeviceCollection());

						// Save app settings to disk
						Config.AppSettings?.SaveChanges();

						// Save rules before exiting
						var rulesService = Tools.ResolveIfNull<IRuleService>(null!);
						rulesService.SaveRules();
					};

					StaticData.MainWindow = desktop.MainWindow;
					mainViewModel.InitTrayIcon();
				}

				base.OnFrameworkInitializationCompleted();
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), this.GetType(), e.Message);
				throw;
			}
		}
	}
}