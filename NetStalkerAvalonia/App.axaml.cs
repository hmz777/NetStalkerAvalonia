using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.Views;
using Serilog;
using System;

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
				if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{
					var adapterSelectWindow = new AdapterSelectView
					{
						DataContext = Tools.ResolveIfNull<AdapterSelectViewModel>(null!)
					};

					desktop.MainWindow = adapterSelectWindow;

					adapterSelectWindow.ViewModel!.Accept.Subscribe(x =>
					{
						var mainViewModel = Tools.ResolveIfNull<MainViewModel>(null!);

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

						// Switch the main viewmodel after the initial adapter setup

						desktop.MainWindow = new MainView
						{
							DataContext = mainViewModel
						};

						DataContext = mainViewModel;

						StaticData.MainWindow = desktop.MainWindow;

						desktop.MainWindow.Show();
						adapterSelectWindow.Close();

						mainViewModel.InitTrayIcon();
					});
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