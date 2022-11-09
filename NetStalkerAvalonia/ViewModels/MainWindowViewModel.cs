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
        #region Members

        private bool _blockAllHandlerAttached;
        private IDisposable? _blockAllFutureHandlerSubscription = null;
        private bool _redirectAllHandlerAttached;
        private IDisposable? _redirectAllFutureHandlerSubscription = null;

        #endregion

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

        public ReactiveCommand<PhysicalAddress?, Unit> BlockUnblock { get; }
        public ReactiveCommand<PhysicalAddress?, Unit> RedirectUnRedirect { get; }
        public ReactiveCommand<PhysicalAddress?, Unit> Limit { get; }
        public ReactiveCommand<PhysicalAddress?, Unit> SetFriendlyName { get; }
        public ReactiveCommand<PhysicalAddress?, Unit> ClearFriendlyName { get; }

        #endregion

        #region Interactions

        public Interaction<DeviceLimitsModel?, DeviceLimitsModel?> ShowLimitDialogInteraction { get; set; }
        public Interaction<StatusMessage, Unit> ShowStatusMessageInteraction { get; set; }
        public Interaction<string?, string?> SetFriendlyNameInteraction { get; set; }

        #endregion

        #region UI Bounded Properties

        private Device? _selectedDevice;

        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }

        private bool _allBlocked;

        public bool AllBlocked
        {
            get => _allBlocked;
            set => this.RaiseAndSetIfChanged(ref _allBlocked, value);
        }

        private bool _allRedirected;

        public bool AllRedirected
        {
            get => _allRedirected;
            set => this.RaiseAndSetIfChanged(ref _allRedirected, value);
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
                () => Router.Navigate.Execute(Tools.ViewModels[1] as IRoutableViewModel));

            GoToOptions = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(Tools.ViewModels[2] as IRoutableViewModel));

            GoToRules = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(Tools.ViewModels[3] as IRoutableViewModel));

            GoToHelp = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(Tools.ViewModels[4] as IRoutableViewModel));

            GoToAbout = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(Tools.ViewModels[5] as IRoutableViewModel));

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

            Scan = ReactiveCommand.Create(ScanImpl);
            Refresh = ReactiveCommand.Create(RefreshImpl);
            BlockAll = ReactiveCommand.CreateFromTask<bool>(BlockAllImpl);
            RedirectAll = ReactiveCommand.CreateFromTask<bool>(RedirectAllImpl);

            #endregion

            #region Context Menu command wiring

            BlockUnblock = ReactiveCommand.CreateFromTask<PhysicalAddress?>(BlockUnblockImpl);
            RedirectUnRedirect = ReactiveCommand.CreateFromTask<PhysicalAddress?>(RedirectUnRedirectImpl);

            #region Limit Dialog

            ShowLimitDialogInteraction = new Interaction<DeviceLimitsModel?, DeviceLimitsModel?>();
            Limit = ReactiveCommand.CreateFromTask<PhysicalAddress?>(LimitImpl);

            #endregion

            #region Friendly Target Name

            SetFriendlyNameInteraction = new Interaction<string?, string?>();
            SetFriendlyName = ReactiveCommand.CreateFromTask<PhysicalAddress?>(SetFriendlyNameImpl);
            ClearFriendlyName = ReactiveCommand.CreateFromTask<PhysicalAddress?>(ClearFriendlyNameImpl);

            #endregion

            #endregion

            #region Status message

            ShowStatusMessageInteraction = new Interaction<StatusMessage, Unit>();

            // This message bus listener is used for displaying status messages by other components in the app
            MessageBus
                .Current
                .Listen<StatusMessage>(ContractKeys.StatusMessage.ToString())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => ShowStatusMessage(x))
                .Subscribe();

            #endregion

            #region Exception Handling

            Scan.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            Refresh.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            BlockUnblock.ThrownExceptions.Subscribe(x =>
                Tools.HandleError(ShowStatusMessageInteraction, new StatusMessage(MessageType.Error, x.Message)));
            RedirectUnRedirect.ThrownExceptions.Subscribe(x =>
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

        private void ScanImpl()
        {
            ResolveRequiredServices();

            _scanner?.Scan();
        }

        private void RefreshImpl()
        {
            ResolveRequiredServices();

            // Send a round of ARP packets to discover devices quicker
            Task.Run(() => _scanner?.Refresh());
        }

        private async Task BlockUnblockImpl(PhysicalAddress? mac)
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

        private async Task RedirectUnRedirectImpl(PhysicalAddress? mac)
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

        private async Task LimitImpl(PhysicalAddress? mac)
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

        private async Task BlockAllImpl(bool active)
        {
            if (AllRedirected && active)
            {
                AllBlocked = false;

                await ShowStatusMessageInteraction
                    .Handle(
                        new StatusMessage(MessageType.Error,
                            "You have to uncheck the Redirect All toggle first!"));
                return;
            }

            if (active)
            {
                var devices = _devicesReadOnly
                    .Where(d => d.IsGateway() == false && d.IsLocalDevice() == false & d.Blocked == false)
                    .ToList();

                foreach (var device in devices)
                {
                    _blockerRedirector?.Block(device.Mac);
                }

                // Attach handler to block all future detections
                if (_blockAllHandlerAttached == false)
                {
                    AttachBlockAllFutureDetectionsHandler();
                    _blockAllHandlerAttached = true;
                }
            }
            else
            {
                var devices = _devicesReadOnly
                    .Where(d => d.IsGateway() == false && d.IsLocalDevice() == false & d.Blocked == true)
                    .ToList();

                foreach (var device in devices)
                {
                    _blockerRedirector?.UnBlock(device.Mac);
                }

                // Remove handler to block all future detections
                if (_blockAllHandlerAttached == true)
                {
                    RemoveBlockAllFutureDetectionsHandler();
                    _blockAllHandlerAttached = false;
                }
            }

            AllBlocked = active;
        }

        private async Task RedirectAllImpl(bool active)
        {
            if (AllBlocked && active)
            {
                AllRedirected = false;

                await ShowStatusMessageInteraction
                    .Handle(
                        new StatusMessage(MessageType.Error,
                            "You have to uncheck the Block All toggle first!"));
                return;
            }

            if (active)
            {
                var devices = _devicesReadOnly
                    .Where(d => d.IsGateway() == false && d.IsLocalDevice() == false && d.Redirected == false)
                    .ToList();

                foreach (var device in devices)
                {
                    _blockerRedirector?.Redirect(device.Mac);
                }

                // Attach handler to redirect all future detections
                if (_redirectAllHandlerAttached == false)
                {
                    AttachRedirectAllFutureDetectionsHandler();
                    _redirectAllHandlerAttached = true;
                }
            }
            else
            {
                var devices = _devicesReadOnly
                    .Where(d => d.IsGateway() == false && d.IsLocalDevice() == false && d.Redirected == true)
                    .ToList();

                foreach (var device in devices)
                {
                    _blockerRedirector?.UnRedirect(device.Mac);
                }

                // Remove handler to redirect all future detections
                if (_redirectAllHandlerAttached == true)
                {
                    RemoveRedirectAllFutureDetectionsHandler();
                    _redirectAllHandlerAttached = false;
                }
            }

            AllRedirected = active;
        }

        private async Task SetFriendlyNameImpl(PhysicalAddress? mac)
        {
            var validationResult = await CheckIfMacAddressIsValidAsync(mac, true);

            if (validationResult.isValid == false)
                return;

            var result =
                await SetFriendlyNameInteraction!.Handle(validationResult.device.Name);

            validationResult.device.SetFriendlyName(result);

            await _deviceNameResolver?.SaveDeviceNamesAsync(_devicesReadOnly.ToList())!;
        }

        private async Task ClearFriendlyNameImpl(PhysicalAddress? mac)
        {
            var validationResult = await CheckIfMacAddressIsValidAsync(mac, true);

            if (validationResult.isValid == false)
                return;

            // It doesn't matter if we specify the second optional parameter or not
            validationResult.device.SetFriendlyName(null!);

            await _deviceNameResolver?.SaveDeviceNamesAsync(_devicesReadOnly.ToList())!;
        }

        #endregion

        #region Tools

        //We get the view name, otherwise we return the initial name
        private static string GetPageNameFromViewModel(IRoutableViewModel? routableViewModel)
        {
            return routableViewModel?.UrlPathSegment ?? "Device List";
        }

        // canBeAppliedToGatewayAndLocal parameter means friendly commands like setting device name, clear name, etc.
        private async Task<(bool isValid, Device device)> CheckIfMacAddressIsValidAsync(PhysicalAddress? mac,
            bool canBeAppliedToGatewayAndLocal = false)
        {
            var device = _devicesReadOnly.FirstOrDefault(x => x.Mac!.Equals(mac));

            if (device == null)
            {
                await ShowStatusMessageInteraction!.Handle(new StatusMessage(MessageType.Error,
                    "No device is selected!"));

                return (false, null!);
            }
            else if (device!.IsGateway() && canBeAppliedToGatewayAndLocal == false)
            {
                await ShowStatusMessageInteraction!.Handle(new StatusMessage(MessageType.Error,
                    "Gateway can't be targeted!"));

                return (false, null!);
            }
            else if (device!.IsLocalDevice() && canBeAppliedToGatewayAndLocal == false)
            {
                await ShowStatusMessageInteraction!.Handle(new StatusMessage(MessageType.Error,
                    "You can't target your own device!"));

                return (false, null!);
            }

            return (true, device);
        }

        public IEnumerable<Device> GetUiDeviceCollection() => _devicesReadOnly;

        private void AttachBlockAllFutureDetectionsHandler()
        {
            _blockAllFutureHandlerSubscription =
                _devicesReadOnly
                    .ToObservableChangeSet()
                    .Where(change => change.Adds > 0)
                    .ToCollection()
                    .Select(x => BlockAllImpl(true))
                    .Subscribe();
        }

        private void RemoveBlockAllFutureDetectionsHandler()
        {
            _blockAllFutureHandlerSubscription?.Dispose();
        }

        private void AttachRedirectAllFutureDetectionsHandler()
        {
            _redirectAllFutureHandlerSubscription =
                _devicesReadOnly
                    .ToObservableChangeSet()
                    .Where(change => change.Adds > 0)
                    .ToCollection()
                    .Select(x => RedirectAllImpl(true))
                    .Subscribe();
        }

        private void RemoveRedirectAllFutureDetectionsHandler()
        {
            _redirectAllFutureHandlerSubscription?.Dispose();
        }

        private async Task<Unit> ShowStatusMessage(StatusMessage statusMessage)
        {
            await ShowStatusMessageInteraction.Handle(statusMessage);
            return Unit.Default;
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
            var encText = Tools.StringEncrypt("This is a secret!");
            //var decText = Tools.StringDecrypt("This is a secret!");
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