using DynamicData;
using DynamicData.Binding;
using NetStalkerAvalonia.Compairers;
using NetStalkerAvalonia.Components.DeviceList;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;

namespace NetStalkerAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        #region Routing

        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        // Commands to navigate the different views
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToHome { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToRules { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToSniffer { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToOptions { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToHelp { get; }

        // The command that navigates a user back.
        public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;
        public void GoBackHandler() { GoBack?.Execute(); }

        private readonly ObservableAsPropertyHelper<bool> _canGoBack;
        public bool CanGoBack => _canGoBack.Value;

        #endregion

        #region UI Info

        private readonly ObservableAsPropertyHelper<string> _pageTitle;
        public string PageTitle => _pageTitle.Value;

        #endregion

        #region Devices List

        // The data store for devices
        private readonly SourceCache<Device, string> devicesStore = new(device => device.Mac.ToString());

        // Collection projected from source for UI
        private readonly ReadOnlyObservableCollection<Device> devicesDisplay;

        // Accessor to expose the UI device list
        public ReadOnlyObservableCollection<Device> Devices => devicesDisplay;

        // Configure the device list view
        public DeviceListViewSettings DeviceListViewSettings { get; set; } = new();

        #endregion

        #region Helpers

        private IEqualityComparer<Device> DeviceEqualityComparer = new DeviceEqualityComparer();

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            // Info wiring
            _pageTitle = this.WhenAnyObservable(x => x.Router.CurrentViewModel)
                             .Select(x => GetPageNameFromViewModel(x))
                             .ToProperty(this, x => x.PageTitle);

            // Navigation wiring
            _canGoBack = this.WhenAnyValue(x => x.Router.NavigationStack.Count)
                             .Select(count => count > 0)
                             .ToProperty(this, x => x.CanGoBack);

            GoToSniffer = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new SnifferViewModel(this)));

            GoToOptions = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new OptionsViewModel(this)));

            // Device collection projection for UI
            devicesStore
                   .Connect()
                   .Sort(SortExpressionComparer<Device>.Descending(device => device.DateAdded))
                   .ObserveOn(RxApp.MainThreadScheduler)
                   .Bind(out devicesDisplay)
                   .Subscribe();

            // Testing
            // Dummy data population
            PopulateDummyData(devicesStore);
        }

        #endregion

        #region Event Handlers

        #region Device List

        #endregion

        #endregion

        #region Tools

        //We get the view name, othewise we return the initial name
        private static string GetPageNameFromViewModel(IRoutableViewModel routableViewModel)
        {
            return routableViewModel?.UrlPathSegment ?? "Device List";
        }

        #endregion

        #region Testing

        private void PopulateDummyData(ISourceCache<Device, string> devices)
        {
            for (int i = 0; i < 50; i++)
            {
                devices.AddOrUpdate(
                     new Device(IPAddress.Parse($"192.168.1.{i}"),
                                PhysicalAddress.Parse(GetRandomMacAddress())), DeviceEqualityComparer);
            }
        }

        // For debugging
        public void TestMethod()
        {

        }

        private static string GetRandomMacAddress()
        {
            var random = new Random();
            var buffer = new byte[6];
            random.NextBytes(buffer);
            var result = String.Concat(buffer.Select(x => string.Format("{0}:", x.ToString("X2"))).ToArray());
            return result.TrimEnd(':');
        }

        #endregion
    }
}