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
using Windows.Devices.Display;
using Avalonia.Controls;
using DynamicData.Binding;
using Microsoft.Toolkit.Uwp.Notifications;
using NetStalkerAvalonia.Configuration;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;
using Serilog;

namespace NetStalkerAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        #region Services

        private bool _servicesResolved;

        // Required services
        private IDeviceScanner? _scanner;
        private IBlockerRedirector? _blockerRedirector;
        private IDeviceNameResolver? _deviceNameResolver;

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
        public ReactiveCommand<bool, Unit>? BlockAll { get; }
        public ReactiveCommand<bool, Unit>? RedirectAll { get; }

        #endregion

        #region Context Menu Commands

        public ReactiveCommand<PhysicalAddress?, Unit> BlockUnblock { get; set; }
        public ReactiveCommand<PhysicalAddress?, Unit> RedirectUnredirect { get; set; }
        public ReactiveCommand<PhysicalAddress?, Unit> Limit { get; }
        public ReactiveCommand<PhysicalAddress?, Unit> SetFriendlyName { get; set; }
        public ReactiveCommand<PhysicalAddress?, Unit> ClearFriendlyName { get; set; }

        #endregion

        #region Interactions

        public Interaction<DeviceLimitsModel?, DeviceLimitsModel?>? ShowLimitDialogInteraction { get; set; }
        public Interaction<StatusMessage, Unit>? ShowStatusMessageInteraction { get; set; }
        public Interaction<string?, string?>? SetFriendlyNameInteraction { get; set; }

        #endregion

        #region UI Bounded Properties

        private Device? _selectedDevice;

        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }

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

        private IEqualityComparer<Device> _deviceEqualityComparer = new DeviceEqualityComparer();

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            #region Page info wiring

            // Info wiring
            _pageTitle = this.WhenAnyObservable(x => x.Router.CurrentViewModel)
                .Select(GetPageNameFromViewModel)
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
                .Listen<IChangeSet<Device, string>>(ContractKeys.ScannerStream.ToString())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _devicesReadOnly)
                .DisposeMany()
                .Subscribe();

            MessageBus
                .Current
                .RegisterMessageSource(_devicesReadOnly.ToObservableChangeSet(), ContractKeys.UiStream.ToString());

            #endregion

            #region Nav Command wiring

            Scan = ReactiveCommand.Create(ScanForDevices);
            Refresh = ReactiveCommand.Create(RefreshDevices);
            BlockAll = ReactiveCommand.Create<bool>(BlockAllHandler);
            RedirectAll = ReactiveCommand.Create<bool>(RedirectAllHandler);

            #endregion

            #region Context Menu command wiring

            BlockUnblock = ReactiveCommand.CreateFromTask<PhysicalAddress?>(BlockDevice);
            RedirectUnredirect = ReactiveCommand.CreateFromTask<PhysicalAddress?>(RedirectDevice);

            #region Limit Dialog

            ShowLimitDialogInteraction = new Interaction<DeviceLimitsModel?, DeviceLimitsModel?>();
            Limit = ReactiveCommand.CreateFromTask<PhysicalAddress?>(DeviceLimitation);

            #endregion

            #region Friendly Target Name

            SetFriendlyNameInteraction = new Interaction<string?, string?>();
            SetFriendlyName = ReactiveCommand.CreateFromTask<PhysicalAddress?>(SetDeviceFriendlyName);
            ClearFriendlyName = ReactiveCommand.CreateFromTask<PhysicalAddress?>(ClearDeviceFriendlyName);

            #endregion

            #endregion

            #region Status message

            ShowStatusMessageInteraction = new Interaction<StatusMessage, Unit>();

            #endregion

            #region Exception Handling

            Scan.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            Refresh.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            BlockUnblock.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            RedirectUnredirect.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            BlockAll.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            RedirectAll.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));

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
                _deviceNameResolver = Tools.ResolveIfNull<IDeviceNameResolver>(null!);
                _servicesResolved = true;
            }
        }

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

        private async Task BlockDevice(PhysicalAddress? mac)
        {
            var validationResult = await CheckIfMacAddressIsValidAsync(mac);

            if (validationResult.isValid == false)
                return;

            if (validationResult.device.Blocked == false)
            {
                _blockerRedirector?
                    .Block(mac!);
            }
            else
            {
                _blockerRedirector?
                    .UnBlock(mac!);
            }
        }

        private async Task RedirectDevice(PhysicalAddress? mac)
        {
            var validationResult = await CheckIfMacAddressIsValidAsync(mac);

            if (validationResult.isValid == false)
                return;

            if (validationResult.device.Redirected == false)
            {
                _blockerRedirector?.Redirect(mac!);
            }
            else
            {
                _blockerRedirector?.UnRedirect(mac!);
            }
        }

        private async Task DeviceLimitation(PhysicalAddress? mac)
        {
            var validationResult = await CheckIfMacAddressIsValidAsync(mac);

            if (validationResult.isValid == false)
                return;

            var device = validationResult.device;

            var result =
                await ShowLimitDialogInteraction!.Handle(new DeviceLimitsModel(device.DownloadCap / 1024,
                    device.UploadCap / 1024));

            if (result != null)
            {
                _blockerRedirector?.Limit(mac!, result.Download, result.Upload);
            }
        }

        private void BlockAllHandler(bool active)
        {
            var devices = _devicesReadOnly
                .Where(d => d.IsGateway() == false && d.IsLocalDevice() == false)
                .ToList();

            if (active)
            {
                foreach (var device in devices)
                {
                    _blockerRedirector?.Block(device.Mac);
                }
            }
            else
            {
                foreach (var device in devices)
                {
                    _blockerRedirector?.UnBlock(device.Mac);
                }
            }
        }

        private void RedirectAllHandler(bool active)
        {
            var devices = _devicesReadOnly
                .Where(d => d.IsGateway() == false && d.IsLocalDevice() == false)
                .ToList();

            if (active)
            {
                foreach (var device in devices)
                {
                    _blockerRedirector?.Redirect(device.Mac);
                }
            }
            else
            {
                foreach (var device in devices)
                {
                    _blockerRedirector?.UnRedirect(device.Mac);
                }
            }
        }

        private async Task SetDeviceFriendlyName(PhysicalAddress? mac)
        {
            var validationResult = await CheckIfMacAddressIsValidAsync(mac);

            if (validationResult.isValid == false)
                return;

            var result =
                await SetFriendlyNameInteraction!.Handle(validationResult.device.Name);

            validationResult.device.SetFriendlyName(result);

            await _deviceNameResolver?.SaveDeviceNamesAsync(_devicesReadOnly.ToList())!;
        }

        private async Task ClearDeviceFriendlyName(PhysicalAddress? mac)
        {
            var validationResult = await CheckIfMacAddressIsValidAsync(mac);

            if (validationResult.isValid == false)
                return;

            validationResult.device.ClearFriendlyName();
            
            await _deviceNameResolver?.SaveDeviceNamesAsync(_devicesReadOnly.ToList())!;
        }

        #endregion

        #region Tools

        //We get the view name, otherwise we return the initial name
        private static string GetPageNameFromViewModel(IRoutableViewModel? routableViewModel)
        {
            return routableViewModel?.UrlPathSegment ?? "Device List";
        }

        private async Task<(bool isValid, Device device)> CheckIfMacAddressIsValidAsync(PhysicalAddress? mac)
        {
            var device = _devicesReadOnly.FirstOrDefault(x => x.Mac!.Equals(mac));

            if (device == null)
            {
                await ShowStatusMessageInteraction!.Handle(new StatusMessage(MessageType.Error,
                    "No device is selected!"));

                return (false, null!);
            }
            else if (device!.IsGateway())
            {
                await ShowStatusMessageInteraction!.Handle(new StatusMessage(MessageType.Error,
                    "Gateway can't be targeted!"));

                return (false, null!);
            }
            else if (device!.IsLocalDevice())
            {
                await ShowStatusMessageInteraction!.Handle(new StatusMessage(MessageType.Error,
                    "You can't target your own device!"));

                return (false, null!);
            }

            return (true, device);
        }

        #endregion

        #region Testing

        private void PopulateDummyData(ISourceCache<Device, string> devices)
        {
            for (int i = 0; i < 50; i++)
            {
                devices.AddOrUpdate(
                    new Device(IPAddress.Parse($"192.168.1.{i}"),
                        PhysicalAddress.Parse(GetRandomMacAddress())), _deviceEqualityComparer);
            }
        }

        // For debugging
        public void TestMethod(Device? device = null)
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