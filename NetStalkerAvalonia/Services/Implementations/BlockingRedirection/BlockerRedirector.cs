using NetStalkerAvalonia.Models;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using NetStalkerAvalonia.Configuration;
using NetStalkerAvalonia.Helpers;
using PacketDotNet;
using ReactiveUI;
using Serilog;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetStalkerAvalonia.Services.Implementations.BlockingRedirection
{
    public class BlockerRedirector : IBlockerRedirector
    {
        #region Members

        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isStarted;
        private LibPcapLiveDevice? _device;

        private ReadOnlyObservableCollection<Device>? _clients;

        private readonly ILogger? _logger;

        #endregion

        #region Constructor

        public BlockerRedirector(ILogger logger = null!)
        {
            _logger = Tools.ResolveIfNull(logger);

            InitDevice();
            BindClients();

            _logger.Information("Service of type: {Type}, initialized",
                typeof(IBlockerRedirector));
        }

        #endregion

        #region Internal

        private void InitDevice()
        {
            if (_device == null)
            {
                var adapterName = (from devicex in LibPcapLiveDeviceList.Instance
                    where devicex.Interface.FriendlyName == HostInfo.NetworkAdapterName
                    select devicex).ToList()[0].Name;

                _device = LibPcapLiveDeviceList.New()[adapterName];
                _device.Open(DeviceModes.Promiscuous, 1000);
                _device.Filter = "ether proto \\ip";
                _device.OnPacketArrival += DeviceOnOnPacketArrival;
            }
        }

        private void BindClients()
        {
            MessageBus
                .Current
                .Listen<IChangeSet<Device>>(ContractKeys.UiStream.ToString())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _clients)
                .DisposeMany()
                .Subscribe();
        }

        private void DeviceOnOnPacketArrival(object sender, PacketCapture e)
        {
            if (_isStarted == false)
                return;

            var rawCapture = e.GetPacket();
            var packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data) as EthernetPacket;

            if (packet == null)
                return;

            var outTarget = _clients?
                .FirstOrDefault(x => x.Mac!.Equals(packet.SourceHardwareAddress));

            if (outTarget is not null
                && outTarget is { Redirected: true }
                && outTarget.IsLocalDevice() == false
                && outTarget.IsGateway() == false)
            {
                if (outTarget.UploadCap == 0 || outTarget.UploadCap > outTarget.BytesSentSinceLastReset)
                {
                    packet.SourceHardwareAddress = HostInfo.HostMac;
                    packet.DestinationHardwareAddress = HostInfo.GatewayMac;
                    _device.SendPacket(packet);
                    outTarget.SetSentBytes(packet.Bytes.Length);
                }
            }
            else if (packet.SourceHardwareAddress.Equals(HostInfo.GatewayMac))
            {
                var ipv4Packet = packet.Extract<IPv4Packet>();
                var inTarget = _clients?
                    .FirstOrDefault(x => x.Ip!.Equals(ipv4Packet.DestinationAddress));

                if (inTarget is not null
                    && inTarget is { Redirected: true }
                    && inTarget.IsLocalDevice() == false
                    && inTarget.IsGateway() == false)
                {
                    if (inTarget.DownloadCap == 0 || inTarget.DownloadCap > inTarget.BytesReceivedSinceLastReset)
                    {
                        packet.SourceHardwareAddress = HostInfo.HostMac;
                        packet.DestinationHardwareAddress = inTarget.Mac;
                        _device.SendPacket(packet);
                        inTarget.SetReceivedBytes(packet.Bytes.Length);
                    }
                }
            }
        }

        private void StartIfNotStarted()
        {
            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            else if (_cancellationTokenSource.IsCancellationRequested)
            {
                if (_cancellationTokenSource.TryReset() == false)
                {
                    throw new InvalidOperationException("Can not reset the BlockerRedirector's cancellation token.");
                }
            }

            if (_isStarted == false)
            {
                if (_device is not null && _device is { Opened: true, Started: false })
                {
                    _device.StartCapture();

                    Task.Run(async () =>
                    {
                        while (_cancellationTokenSource.IsCancellationRequested == false)
                        {
                            SpoofClients();

                            await Task.Delay(1000);
                        }
                    });

                    _isStarted = true;
                }
            }


            _logger!.Information("Service of type: {Type}, started",
                typeof(IBlockerRedirector));
        }

        private void SpoofClients()
        {
            foreach (var client in _clients!)
            {
                if ((client.Blocked || client.Redirected)
                    && client.IsLocalDevice() == false
                    && client.IsGateway() == false)
                {
                    ConstructAndSendArp(client, ArpPacketType.Spoof);

                    try
                    {
                        if (ConfigurationManager
                                .AppSettings[nameof(ConfigKeys.SpoofProtection)] == "false")
                            ConstructAndSendArp(client, ArpPacketType.Protection);
                    }
                    catch (Exception e)
                    {
                        _logger!.Error(e, "Exception of type {Type} triggered with {Message}",
                            e.GetType(), e.Message);
                    }
                }
            }
        }

        private void ConstructAndSendArp(Device device, ArpPacketType arpPacketType)
        {
            switch (arpPacketType)
            {
                case ArpPacketType.Spoof:
                    var arpPacketForVicSpoof = new ArpPacket(ArpOperation.Request,
                        targetHardwareAddress: device.Mac,
                        targetProtocolAddress: device.Ip,
                        senderHardwareAddress: HostInfo.HostMac,
                        senderProtocolAddress: HostInfo.GatewayIp);

                    var etherPacketForVicSpoof = new EthernetPacket(
                        sourceHardwareAddress: HostInfo.HostMac,
                        destinationHardwareAddress: device.Mac,
                        EthernetType.Arp)
                    {
                        PayloadPacket = arpPacketForVicSpoof
                    };

                    var arpPacketForGatewaySpoof = new ArpPacket(ArpOperation.Request,
                        targetHardwareAddress: HostInfo.GatewayMac,
                        targetProtocolAddress: HostInfo.GatewayIp,
                        senderHardwareAddress: HostInfo.HostMac,
                        senderProtocolAddress: device.Ip);

                    var etherPacketForGatewaySpoof = new EthernetPacket(
                        sourceHardwareAddress: HostInfo.HostMac,
                        destinationHardwareAddress: HostInfo.GatewayMac,
                        EthernetType.Arp)
                    {
                        PayloadPacket = arpPacketForGatewaySpoof
                    };

                    if (device.Blocked)
                        _device.SendPacket(etherPacketForVicSpoof);

                    if (device.Redirected)
                        _device.SendPacket(etherPacketForGatewaySpoof);

                    break;
                case ArpPacketType.Protection:
                    var arpPacketForVicProtection = new ArpPacket(ArpOperation.Response,
                        targetHardwareAddress: HostInfo.HostMac,
                        targetProtocolAddress: HostInfo.HostIp,
                        senderHardwareAddress: device.Mac,
                        senderProtocolAddress: device.Ip);

                    var etherPacketForVicProtection = new EthernetPacket(
                        sourceHardwareAddress: device.Mac,
                        destinationHardwareAddress: HostInfo.HostMac,
                        EthernetType.Arp)
                    {
                        PayloadPacket = arpPacketForVicProtection
                    };

                    var arpPacketForGatewayProtection = new ArpPacket(ArpOperation.Response,
                        targetHardwareAddress: HostInfo.HostMac,
                        targetProtocolAddress: HostInfo.HostIp,
                        senderHardwareAddress: HostInfo.GatewayMac,
                        senderProtocolAddress: HostInfo.GatewayIp);

                    var etherPacketForGatewayProtection = new EthernetPacket(
                        sourceHardwareAddress: HostInfo.GatewayMac,
                        destinationHardwareAddress: HostInfo.HostMac,
                        EthernetType.Arp)
                    {
                        PayloadPacket = arpPacketForGatewayProtection
                    };

                    _device.SendPacket(etherPacketForGatewayProtection);
                    _device.SendPacket(etherPacketForVicProtection);
                    break;
            }
        }

        private void TryPauseIfNoDevicesLeft()
        {
            if ((bool)_clients?.Any(client => client.Blocked || client.Redirected) == false)
            {
                // If no clients have active blocking or redirection we pause the service 
                // so we don't do extra work on idle
                Stop();

                _logger!.Information("Service of type: {Type}, paused",
                    typeof(IBlockerRedirector));
            }
        }

        private void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _isStarted = false;
            _device?.StopCapture();

            _logger!.Information("Service of type: {Type}, stopped",
                typeof(IBlockerRedirector));
        }

        #endregion

        #region API

        public bool IsStarted => _isStarted;

        public void Block(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.Block();
            StartIfNotStarted();

            _logger!.Information("Service of type: {Type}, Block device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.Ip);
        }

        public void Redirect(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.Redirect();
            StartIfNotStarted();

            _logger!.Information("Service of type: {Type}, Redirect device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.Ip);
        }

        public void UnBlock(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.UnBlock();
            TryPauseIfNoDevicesLeft();

            _logger!.Information("Service of type: {Type}, Unblock device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.Ip);
        }

        public void UnRedirect(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.UnRedirect();
            TryPauseIfNoDevicesLeft();

            _logger!.Information("Service of type: {Type}, Unredirect device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.Ip);
        }

        public void Limit(Device device, int download, int upload)
        {
            Redirect(device);

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.SetDownloadCap(download);
            brDevice.SetUploadCap(upload);

            _logger!.Information("Service of type: {Type}, Limit device with MAC:{Mac} - IP:{Ip} - " +
                                 "Download: {Download} - Upload: {Upload}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.Ip,
                download,
                upload);
        }

        public void LimitDownload(Device device, int download)
        {
            Redirect(device);

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.SetDownloadCap(download);

            _logger!.Information("Service of type: {Type}, Limit device with MAC:{Mac} - IP:{Ip} - " +
                                 "Download: {Download}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.Ip,
                download);
        }

        public void LimitUpload(Device device, int upload)
        {
            Redirect(device);

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.SetUploadCap(upload);

            _logger!.Information("Service of type: {Type}, Limit device with MAC:{Mac} - IP:{Ip} - " +
                                 "Upload: {Upload}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.Ip,
                upload);
        }

        public void ClearLimits(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.SetDownloadCap(0);
            brDevice.SetUploadCap(0);

            _logger!.Information("Service of type: {Type}, Clear device limits",
                typeof(IBlockerRedirector));
        }

        public void ClearDownload(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.SetDownloadCap(0);

            _logger!.Information("Service of type: {Type}, Clear device download limit",
                typeof(IBlockerRedirector));
        }

        public void ClearUpload(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(device.Mac))
                .First();

            brDevice.SetUploadCap(0);

            _logger!.Information("Service of type: {Type}, Clear device upload limit",
                typeof(IBlockerRedirector));
        }

        public void Dispose()
        {
            Stop();

            if (_device != null)
            {
                _device.Close();
                _device.OnPacketArrival -= DeviceOnOnPacketArrival;
                _device.Dispose();
                _device = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }

            _logger!.Information("Service of type: {Type}, disposed",
                typeof(IBlockerRedirector));
        }

        #endregion
    }
}