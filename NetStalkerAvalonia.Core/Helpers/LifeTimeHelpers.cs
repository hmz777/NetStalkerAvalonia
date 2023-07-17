using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.ViewModels;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;
using NetStalkerAvalonia.Core.Views;
using System;

namespace NetStalkerAvalonia.Core.Helpers
{
	public class LifeTimeHelpers
	{
		public static void ConfigureAppStart(Application application)
		{
			var desktop = application.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

			var adapterSelectWindow = new AdapterSelectView
			{
				DataContext = DependencyInjectionHelpers.ResolveIfNull<AdapterSelectViewModel>(null!)
			};

			desktop!.MainWindow = adapterSelectWindow;

			adapterSelectWindow.ViewModel!.Accept.Subscribe(x =>
			{
				var mainViewModel = DependencyInjectionHelpers.ResolveIfNull<MainViewModel>(null!);
				application.DataContext = mainViewModel;

				desktop.ShutdownRequested += (sender, args) =>
				{
					var homeViewModel = DependencyInjectionHelpers.ResolveIfNull<HomeViewModel>(null!);

					// Save device friendly names before exiting
					var deviceNameResolver = DependencyInjectionHelpers.ResolveIfNull<IDeviceNameResolver>(null!);
					deviceNameResolver.SaveDeviceNamesAsync(homeViewModel.GetUiDeviceCollection()); 
					// TODO: Abstract the device list into its own service

					// Save app settings to disk
					Config.AppSettings?.SaveChanges();

					// Save rules before exiting
					var rulesService = DependencyInjectionHelpers.ResolveIfNull<IRuleService>(null!);
					rulesService.SaveRules();
				};

				// Switch the main view model after the initial adapter setup

				desktop.MainWindow = new MainView
				{
					DataContext = mainViewModel,
					ViewModel = mainViewModel
				};

				StaticData.MainWindow = desktop.MainWindow;

				desktop.MainWindow.Show();
				adapterSelectWindow.Close();

				mainViewModel.InitTrayIcon();
			});
		}

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
