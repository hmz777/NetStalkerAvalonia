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
				if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{
					var adapterSelectWindow = new AdapterSelectWindow
					{
						DataContext = new AdapterSelectViewModel(null!)
					};

					desktop.MainWindow = adapterSelectWindow;

					adapterSelectWindow.ViewModel!.Accept.Subscribe(x =>
					{
						StaticData.InitRoutedViewModels();

						if (StaticData.ViewModels.First() is not MainWindowViewModel mainViewModel)
						{
							throw new Exception("Error initializing view models!");
						}

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

						desktop.MainWindow = new MainWindow
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