using NetStalkerAvalonia.Models;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
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
        private bool _hasSpoofProtection;

        private Timer? _byteCounterTimer;

        private ReadOnlyObservableCollection<Device>? _clients;

        #endregion

        #region Constructor

        public BlockerRedirector()
        {
            InitDevice();
            BindClients();

            _hasSpoofProtection = ConfigurationManager
                .AppSettings[nameof(ConfigKeys.SpoofProtection)] == "true";

            Log.Information("Service of type: {Type}, initialized",
                typeof(IBlockerRedirector));
        }

        #endregion

        #region Init

        private void InitDevice()
        {
            if (_device == null)
            {
                var adapterName = (from devicex in LibPcapLiveDeviceList.Instance
                    where devicex.Interface.FriendlyName == HostInfo.NetworkAdapterName
                    select devicex).ToList()[0].Name;

                _device = LibPcapLiveDeviceList.New()[adapterName];
                _device.Open(DeviceModes.Promiscuous, 1000);
                _device.Filter = "ip";
                _device.OnPacketArrival += DeviceOnOnPacketArrival;

                // The state parameter here doesn't matter since we're initializing the timer
                // it will be later started when this service is started
                InitOrToggleByteCounterTimer(true);
            }
        }

        private void InitOrToggleByteCounterTimer(bool state)
        {
            if (_byteCounterTimer == null)
            {
                _byteCounterTimer = new Timer(
                    ByteCounterTimerOnElapsed,
                    null,
                    Timeout.InfiniteTimeSpan,
                    Timeout.InfiniteTimeSpan);
            }
            else if (state)
            {
                _byteCounterTimer.Change(
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(1));
            }
            else
            {
                _byteCounterTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
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

        #endregion

        #region Internal

        private void ByteCounterTimerOnElapsed(object? stateInfo)
        {
            if (_clients != null)
            {
                foreach (var client in _clients)
                {
                    client?.ResetSentBytes();
                    client?.ResetReceivedBytes();
                }
            }
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
                .FirstOrDefault(x => x.Mac.Equals(packet.SourceHardwareAddress));

            if (outTarget is not null
                && outTarget is { Redirected: true })
            {
                if (outTarget.UploadCap == 0 || outTarget.UploadCap > outTarget.BytesSentSinceLastReset)
                {
                    packet.SourceHardwareAddress = HostInfo.HostMac;
                    packet.DestinationHardwareAddress = HostInfo.GatewayMac;
                    _device.SendPacket(packet);
                    outTarget.IncrementSentBytes(packet.Bytes.Length);
                }
            }
            else if (packet.SourceHardwareAddress.Equals(HostInfo.GatewayMac))
            {
                var ipv4Packet = packet.Extract<IPv4Packet>();
                var inTarget = _clients?
                    .FirstOrDefault(x => x.Ip!.Equals(ipv4Packet.DestinationAddress));

                if (inTarget is not null
                    && inTarget is { Redirected: true })
                {
                    if (inTarget.DownloadCap == 0 || inTarget.DownloadCap > inTarget.BytesReceivedSinceLastReset)
                    {
                        packet.SourceHardwareAddress = HostInfo.HostMac;
                        packet.DestinationHardwareAddress = inTarget.Mac;
                        _device.SendPacket(packet);
                        inTarget.IncrementReceivedBytes(packet.Bytes.Length);
                    }
                }
            }
        }

        private void StartIfNotStarted()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            else
            {
                if (_cancellationTokenSource.TryReset() == false)
                    throw new InvalidOperationException("Can not reset the BlockerRedirector's cancellation token.");
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

                    InitOrToggleByteCounterTimer(true);

                    _isStarted = true;
                }
            }

            Log.Information("Service of type: {Type}, started",
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
                        if (_hasSpoofProtection)
                            ConstructAndSendArp(client, ArpPacketType.Protection);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Exception of type {Type} triggered with message:{Message}",
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
                    SpoofTarget();

                    if (device.Redirected)
                    {
                        SpoofGateway();
                    }

                    break;
                case ArpPacketType.Protection:
                    MaintainTarget();
                    MaintainGateway();
                    break;
            }

            void SpoofTarget()
            {
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
                _device.SendPacket(etherPacketForVicSpoof);
            }

            void SpoofGateway()
            {
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

                _device.SendPacket(etherPacketForGatewaySpoof);
            }

            void MaintainTarget()
            {
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

                _device.SendPacket(etherPacketForVicProtection);
            }

            void MaintainGateway()
            {
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
            }
        }

        private void TryPauseIfNoDevicesLeft()
        {
            if ((bool)_clients?.Any(client => client.Blocked || client.Redirected) == false)
            {
                // If no clients have active blocking or redirection we pause the service 
                // so we don't do extra work on idle
                Stop();

                Log.Information("Service of type: {Type} stopped, Reason: No devices are active",
                    typeof(IBlockerRedirector));
            }
        }

        private void Stop()
        {
            _cancellationTokenSource?.Cancel();
            InitOrToggleByteCounterTimer(false);
            _isStarted = false;
            _device?.StopCapture();

            Log.Information("Service of type: {Type}, stopped",
                typeof(IBlockerRedirector));
        }

        #endregion

        #region API

        public bool Status => _isStarted;

        public void Block(PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            if (brDevice.Redirected)
                brDevice.UnRedirect();

            brDevice.Block();
            StartIfNotStarted();

            Log.Information("Service of type: {Type}, Block device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                brDevice.Mac,
                brDevice.Ip);
        }

        public void Redirect(PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            if (brDevice.Blocked)
                brDevice.UnBlock();

            brDevice.Redirect();
            StartIfNotStarted();

            Log.Information("Service of type: {Type}, Redirect device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                brDevice.Mac,
                brDevice.Ip);
        }

        public void UnBlock(PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.UnBlock();
            TryPauseIfNoDevicesLeft();

            Log.Information("Service of type: {Type}, Unblock device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                brDevice.Mac,
                brDevice.Ip);
        }

        public void UnRedirect(PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.UnRedirect();
            TryPauseIfNoDevicesLeft();

            Log.Information("Service of type: {Type}, Unredirect device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                brDevice.Mac,
                brDevice.Ip);
        }

        public void Limit(PhysicalAddress mac, int download, int upload)
        {
            Redirect(mac);

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.SetDownloadCap(download);
            brDevice.SetUploadCap(upload);

            Log.Information("Service of type: {Type}, Limit device with MAC:{Mac} - IP:{Ip} - " +
                                 "Download: {Download} - Upload: {Upload}",
                typeof(IBlockerRedirector),
                brDevice.Mac,
                brDevice.Ip,
                download,
                upload);
        }

        public void LimitDownload(PhysicalAddress mac, int download)
        {
            Redirect(mac);

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.SetDownloadCap(download);

            Log.Information("Service of type: {Type}, Limit device with MAC:{Mac} - IP:{Ip} - " +
                                 "Download: {Download}",
                typeof(IBlockerRedirector),
                brDevice.Mac,
                brDevice.Ip,
                download);
        }

        public void LimitUpload(PhysicalAddress mac, int upload)
        {
            Redirect(mac);

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.SetUploadCap(upload);

            Log.Information("Service of type: {Type}, Limit device with MAC:{Mac} - IP:{Ip} - " +
                                 "Upload: {Upload}",
                typeof(IBlockerRedirector),
                brDevice.Mac,
                brDevice.Ip,
                upload);
        }

        public void ClearLimits(PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.SetDownloadCap(0);
            brDevice.SetUploadCap(0);

            Log.Information("Service of type: {Type}, Limits cleared for device with MAC:{Mac}",
                typeof(IBlockerRedirector), brDevice.Mac);
        }

        public void ClearDownload(PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.SetDownloadCap(0);

            Log.Information("Service of type: {Type}, Clear download limit for device with MAC:{Mac}",
                typeof(IBlockerRedirector), brDevice.Mac);
        }

        public void ClearUpload(PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            var brDevice = _clients!
                .Where(d => d.Mac!.Equals(mac))
                .First();

            brDevice.SetUploadCap(0);

            Log.Information("Service of type: {Type}, Clear upload limit for device with MAC:{Mac}",
                typeof(IBlockerRedirector), brDevice.Mac);
        }

        public void Dispose()
        {
            Stop();

            if (_device != null)
            {
                _byteCounterTimer?.Dispose();
                _device.Close();
                _device.OnPacketArrival -= DeviceOnOnPacketArrival;
                _device.Dispose();
                _device = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }

            Log.Information("Service of type: {Type}, disposed",
                typeof(IBlockerRedirector));
        }

        #endregion
    }
}