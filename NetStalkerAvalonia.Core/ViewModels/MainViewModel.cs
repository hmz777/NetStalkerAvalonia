using Avalonia;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using NetStalkerAvalonia.Core.Compairers;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Core.Views.Components;
using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Models;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations;
using NetStalkerAvalonia.Core.Services.Implementations.ViewRouting;
using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		#region Services

		// Required services
		private readonly IRouter _router;

		#endregion

		#region ViewModels

		private readonly HomeViewModel _homeViewModel;
		private readonly SnifferViewModel _snifferViewModel;
		private readonly OptionsViewModel _optionsViewModel;
		private readonly RuleBuilderViewModel _ruleBuilderViewModel;
		private readonly HelpViewModel _helpViewModel;
		private readonly AboutViewModel _aboutViewModel;

		public readonly AppLogViewModel _appLogViewModel;

		#endregion

		#region Routing

		// The Router associated with this Screen.
		// Required by the IScreen interface.
		public RoutingState Router => _router.Router;

		// Commands to navigate the different views
		public ReactiveCommand<Unit, IRoutableViewModel> GoToRules { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> GoToSniffer { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> GoToOptions { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> GoToHelp { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> GoToAbout { get; }
		public ReactiveCommand<Unit, IRoutableViewModel> GoToDeviceList { get; }

		public ReactiveCommand<Unit, Unit>? ShowAppLog { get; }

		#endregion

		#region Tray Icon

		private bool _trayIconVisible;
		public bool TrayIconVisible
		{
			get => _trayIconVisible;
			set => this.RaiseAndSetIfChanged(ref _trayIconVisible, value);
		}

		public ReactiveCommand<Unit, Unit>? ShowApp { get; }
		public ReactiveCommand<Unit, Unit>? ExitApp { get; }

		public void InitTrayIcon()
		{
			StaticData.MainWindow!.PropertyChanged += MainWindow_PropertyChanged;
		}

		private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property.Name == nameof(Window.WindowState))
			{
				var state = (WindowState)e.NewValue!;

				if (state == WindowState.Minimized)
				{
					StaticData.MainWindow!.ShowInTaskbar = Config.AppSettings!.MinimizeToTraySetting == false;
					TrayIconVisible = Config.AppSettings!.MinimizeToTraySetting;
				}
				else
				{
					StaticData.MainWindow!.ShowInTaskbar = true;
				}
			}
		}

		#endregion

		#region Interactions
		public Interaction<Unit, Unit> ShowAppLogInteraction { get; set; }

		#endregion

		#region UI Bounded Properties

		private readonly ObservableAsPropertyHelper<string> _pageTitle;
		public string PageTitle => _pageTitle.Value;

		#endregion

		#region Helpers

		private readonly IEqualityComparer<Device> _deviceEqualityComparer = new DeviceEqualityComparer();

		#endregion

		#region Constructor

#if DEBUG

		public MainViewModel()
		{

		}

#endif

		[Splat.DependencyInjectionConstructor]
		public MainViewModel(
			IRouter router,
			HomeViewModel homeViewModel,
			SnifferViewModel snifferViewModel,
			OptionsViewModel optionsViewModel,
			RuleBuilderViewModel ruleBuilderViewModel,
			HelpViewModel helpViewModel,
			AboutViewModel aboutViewModel,
			AppLogViewModel appLogViewModel
			)
		{
			_router = router;
			_homeViewModel = homeViewModel;
			_snifferViewModel = snifferViewModel;
			_optionsViewModel = optionsViewModel;
			_ruleBuilderViewModel = ruleBuilderViewModel;
			_helpViewModel = helpViewModel;
			_aboutViewModel = aboutViewModel;
			_appLogViewModel = appLogViewModel;

			#region Page info wiring

			//Info wiring
			_pageTitle = this.WhenAnyObservable(x => x.Router.CurrentViewModel)
				.Select(GetPageNameFromViewModel)
				.ToProperty(this, x => x.PageTitle);

			#endregion

			#region Navigation wiring

			GoToDeviceList = ReactiveCommand.CreateFromObservable(
				() => Router.Navigate.Execute(homeViewModel));

			GoToSniffer = ReactiveCommand.CreateFromObservable(
				() => Router.Navigate.Execute(snifferViewModel));

			GoToOptions = ReactiveCommand.CreateFromObservable(
				() => Router.Navigate.Execute(optionsViewModel));

			GoToRules = ReactiveCommand.CreateFromObservable(
				() => Router.Navigate.Execute(ruleBuilderViewModel));

			GoToHelp = ReactiveCommand.CreateFromObservable(
				() => Router.Navigate.Execute(helpViewModel));

			GoToAbout = ReactiveCommand.CreateFromObservable(
				() => Router.Navigate.Execute(aboutViewModel));

			#endregion

			#region Tray Icon			

			ShowApp = ReactiveCommand.Create(LifeTimeHelpers.ShowApp);
			ExitApp = ReactiveCommand.Create(LifeTimeHelpers.ExitApp);

			#endregion

			#region App Log

			ShowAppLog = ReactiveCommand.CreateFromTask(ShowAppLogImpl);
			ShowAppLogInteraction = new Interaction<Unit, Unit>();

			#endregion

			Router.NavigateAndReset.Execute(homeViewModel);
		}

		#endregion

		#region Command Handlers

		private async Task ShowAppLogImpl()
		{
			await ShowAppLogInteraction.Handle(Unit.Default);
		}

		#endregion

		#region Tools

		//We get the view name, otherwise we return the initial name
		private static string GetPageNameFromViewModel(IRoutableViewModel? routableViewModel)
		{
			return routableViewModel?.UrlPathSegment ?? "Device List";
		}

		#endregion
	}
}