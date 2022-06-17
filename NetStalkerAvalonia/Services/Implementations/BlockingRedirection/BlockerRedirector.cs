using NetStalkerAvalonia.Models;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
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
                var adapterName = (from devicex in CaptureDeviceList.Instance
                    where ((LibPcapLiveDevice)devicex).Interface.FriendlyName == HostInfo.NetworkAdapterName
                    select devicex).ToList()[0].Name;

                _device = (LibPcapLiveDevice)CaptureDeviceList.New()[adapterName];
                _device.Open(DeviceModes.Promiscuous, 1000);
                _device.Filter = "ether proto \\ip";
                _device.OnPacketArrival += DeviceOnOnPacketArrival;
            }
        }

        private void BindClients()
        {
            this.WhenAnyObservable(x => MessageBus
                    .Current.Listen<IChangeSet<Device, string>>(null))
                .BindTo(this, x => x._clients);
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
                if (outTarget.Upload == 0 || outTarget.Upload > outTarget.BytesSentSinceLastReset)
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
                    .FirstOrDefault(x => x.IP!.Equals(ipv4Packet.DestinationAddress));

                if (inTarget is not null
                    && inTarget is { Redirected: true }
                    && inTarget.IsLocalDevice() == false
                    && inTarget.IsGateway() == false)
                {
                    if (inTarget.Download == 0 || inTarget.Download > inTarget.BytesReceivedSinceLastReset)
                    {
                        packet.SourceHardwareAddress = HostInfo.HostMac;
                        packet.DestinationHardwareAddress = inTarget.Mac;
                        _device.SendPacket(packet);
                        inTarget.SetReceivedBytes(packet.Bytes.Length);
                    }
                }
            }
        }

        private void Start()
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

                    Task.Run(() =>
                    {
                        while (_cancellationTokenSource.IsCancellationRequested == false)
                        {
                            SpoofClients();
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
                if (client.Blocked
                    && client.IsLocalDevice() == false
                    && client.IsGateway() == false)
                {
                    ConstructAndSendArp(client, ArpPacketType.Spoof);
                    if ((bool)ConfigurationManager
                            .GetSection(nameof(ConfigKeys.SpoofProtection)))
                        ConstructAndSendArp(client, ArpPacketType.Protection);
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
                        targetProtocolAddress: device.IP,
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
                        senderProtocolAddress: device.IP);

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
                        senderProtocolAddress: device.IP);

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

        private void Pause()
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

        public void Block(Device device)
        {
            device.Block();
            Start();

            _logger!.Information("Service of type: {Type}, Block device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.IP);
        }

        public void Redirect(Device device)
        {
            device.Redirect();
            Start();

            _logger!.Information("Service of type: {Type}, Redirect device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.IP);
        }

        public void UnBlock(Device device)
        {
            device.UnBlock();
            Pause();

            _logger!.Information("Service of type: {Type}, Unblock device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.IP);
        }

        public void UnRedirect(Device device)
        {
            device.UnRedirect();
            Pause();

            _logger!.Information("Service of type: {Type}, Unredirect device with MAC:{Mac} - IP:{Ip}",
                typeof(IBlockerRedirector),
                device.Mac,
                device.IP);
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