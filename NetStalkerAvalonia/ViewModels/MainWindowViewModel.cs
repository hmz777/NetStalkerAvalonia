using DynamicData;
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
using System.Threading.Tasks;
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
        private IBlockerRedirector? _blockerRedirector;

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

        #region Nav commands

        public ReactiveCommand<Unit, Unit>? Scan { get; }
        public ReactiveCommand<Unit, Unit>? Refresh { get; }

        #endregion

        #region Interactions

        public Interaction<Unit, DeviceLimitResult?>? ShowLimitDialog { get; set; }
        public ReactiveCommand<Unit, Unit> Limit { get; }

        #endregion

        #region UI Bounded Properties

        public Device? SelectedDevice { get; set; }

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
            #region Page info wiring

            // Info wiring
            _pageTitle = this.WhenAnyObservable(x => x.Router.CurrentViewModel)
                .Select(GetPageNameFromViewModel!)
                .ToProperty(this, x => x.PageTitle);

            #endregion

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

            #region Device collection listener

            // Subscribe to the scanner device stream
            // to update the UI list
            MessageBus
                .Current
                .Listen<IChangeSet<Device, string>>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _devicesReadOnly)
                .DisposeMany()
                .Subscribe();

            #endregion

            #region Nav Command wiring

            Scan = ReactiveCommand.Create(ScanForDevices);
            Refresh = ReactiveCommand.Create(RefreshDevices);

            #endregion

            #region Context Menu command wiring

            #endregion

            #region Limit Dialog

            ShowLimitDialog = new Interaction<Unit, DeviceLimitResult?>();
            Limit = ReactiveCommand.CreateFromTask(DeviceLimitation);

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
                _blockerRedirector = Tools.ResolveIfNull<IBlockerRedirector>(null!);
                
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

            // Send a round of ARP packets to discover devices quicker
            Task.Run(() => _scanner?.Refresh());
        }

        private async Task DeviceLimitation()
        {
            var result = await ShowLimitDialog!.Handle(Unit.Default);

            if (result != null)
            {
                SelectedDevice?.SetDownload(result.Download);
                SelectedDevice?.SetUpload(result.Upload);
            }
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