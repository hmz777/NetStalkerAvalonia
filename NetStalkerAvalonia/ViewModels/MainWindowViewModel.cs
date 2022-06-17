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
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;

namespace NetStalkerAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        #region Services

        private bool _servicesResolved;

        // Required services
        private IDeviceScanner? _scanner;
        private IBandwidthController? _bandwidthController;

        #endregion

        #region Routing

        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new();

        // Commands to navigate the different views
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToRules { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToSniffer { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToOptions { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToHelp { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToAbout { get; }

        // The command that navigates a user back.
        public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;
        public void GoBackHandler() => GoBack?.Execute();

        private readonly ObservableAsPropertyHelper<bool> _canGoBack;
        public bool CanGoBack => _canGoBack.Value;

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit>? Scan { get; }
        public ReactiveCommand<Unit, Unit>? Refresh { get; }
        public ReactiveCommand<Device, Unit>? Redirect { get; }
        public ReactiveCommand<Device, Unit>? Block { get; }

        #endregion

        #region UI Info

        private readonly ObservableAsPropertyHelper<string> _pageTitle;
        public string PageTitle => _pageTitle.Value;

        #endregion

        #region Devices List

        // Collection projected from source for UI
        private ReadOnlyObservableCollection<Device> _devicesReadOnly;

        // Accessor to expose the UI device list
        public ReadOnlyObservableCollection<Device> Devices => _devicesReadOnly;

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
                .Select(GetPageNameFromViewModel!)
                .ToProperty(this, x => x.PageTitle);

            #region Navigation wiring

            _canGoBack = this.WhenAnyValue(x => x.Router.NavigationStack.Count)
                .Select(count => count > 0)
                .ToProperty(this, x => x.CanGoBack);

            GoToSniffer = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new SnifferViewModel(this)));

            GoToOptions = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new OptionsViewModel(this)));

            GoToRules = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new RuleBuilderViewModel(this)));

            GoToHelp = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new HelpViewModel(this)));

            GoToAbout = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new AboutViewModel(this)));

            #endregion

            // Subscribe to the scanner device stream
            // to update the UI list
            MessageBus
                .Current
                .Listen<IChangeSet<Device, string>>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _devicesReadOnly)
                .DisposeMany()
                .Subscribe();

            #region Command wiring

            Scan = ReactiveCommand.Create(ScanForDevices);
            Refresh = ReactiveCommand.Create(RefreshDevices);
            Block = ReactiveCommand.Create((Device device) => BlockDevice(device));
            Redirect = ReactiveCommand.Create((Device device) => RedirectDevice(device));

            #endregion
        }

        #endregion

        #region Services

        private void ResolveRequiredServices()
        {
            // Resolve dependencies here
            if (_servicesResolved == false)
            {
                _scanner = Tools.ResolveIfNull<IDeviceScanner>(null!);
                _bandwidthController = Tools.ResolveIfNull<IBandwidthController>(null!);

                _servicesResolved = true;
            }
        }

        #endregion

        #region Event Handlers

        #endregion

        #region Command Handlers

        private void ScanForDevices()
        {
            ResolveRequiredServices();

            _scanner?.Scan();
        }

        private void RefreshDevices()
        {
            ResolveRequiredServices();

            _scanner?.Refresh();
        }

        private void BlockDevice(Device device)
        {
            ResolveRequiredServices();
        }

        private void RedirectDevice(Device device)
        {
            ResolveRequiredServices();
        }

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