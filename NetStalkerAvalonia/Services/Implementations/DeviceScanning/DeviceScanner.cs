using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using NetStalkerAvalonia.Configuration;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using PacketDotNet;
using ReactiveUI;
using Serilog;
using SharpPcap;
using SharpPcap.LibPcap;
using Timer = System.Threading.Timer;

namespace NetStalkerAvalonia.Services.Implementations.DeviceScanning;

public class DeviceScanner : IDeviceScanner
{
    #region Members

    private readonly PacketReceiveTechnique _packetReceiveTechnique;
    private CancellationTokenSource? _cancellationTokenSource;
    private LibPcapLiveDevice? _device;
    private Timer? _discoveryTimer;
    private Timer? _aliveTimer;
    private bool _timerRanFirstTime;

    private readonly ILogger? _logger;
    private readonly IDeviceNameResolver? _deviceNameResolver;
    private readonly IDeviceTypeIdentifier? _deviceTypeIdentifier;

    // This is the original source of clients and all other collections are projections from this
    private SourceCache<Device, string> _clients = new(x => x.Mac!.ToString());

    #endregion

    #region Properties

    public bool IsStarted { get; private set; }

    #endregion

    #region Constructor

    public DeviceScanner(
        PacketReceiveTechnique packetReceiveTechnique = PacketReceiveTechnique.EventHandler,
        IDeviceNameResolver? deviceNameResolver = null!,
        IDeviceTypeIdentifier? deviceTypeIdentifier = null!,
        ILogger? logger = null!)
    {
        _packetReceiveTechnique = packetReceiveTechnique;
        _logger = Tools.ResolveIfNull(logger);

        try
        {
            _deviceNameResolver = Tools.ResolveIfNull(deviceNameResolver);

            if (OptionalFeatures.AvailableFeatures.Contains(typeof(IDeviceTypeIdentifier)))
            {
                _deviceTypeIdentifier = Tools.ResolveIfNull(deviceTypeIdentifier);
            }
        }
        catch (Exception e)
        {
            _logger!.Warning("Service resolve error: {Message}",
                e.Message);
        }

        Init();
        SetupBindings();

        _logger!.Information("Service of type: {Type}, initialized",
            typeof(IDeviceScanner));
    }

    #endregion

    #region Init

    private void Init()
    {
        Tools.ResolveGateway();

        if (_device == null)
        {
            var adapterName = (from deviceX in LibPcapLiveDeviceList.Instance
                where deviceX.Interface.FriendlyName == HostInfo.NetworkAdapterName
                select deviceX).ToList()[0].Name;

            _device = LibPcapLiveDeviceList.New()[adapterName];
            _device.Open(DeviceModes.Promiscuous, 20);
            _device.Filter = "arp";

            // The state parameter here doesn't matter since we're initializing the timer
            // it will be later started when the scan functionality is started
            InitOrToggleDiscoveryTimer(true);
            InitOrToggleAliveTimer(true);

            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void SetupBindings() => MessageBus
        .Current
        .RegisterMessageSource(_clients.Connect(), ContractKeys.ScannerStream.ToString());

    private void InitOrToggleDiscoveryTimer(bool state)
    {
        if (_discoveryTimer == null)
        {
            _discoveryTimer = new Timer(DiscoveryTimerOnElapsed, null,
                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
        else
        {
            if (state)
            {
                if (_timerRanFirstTime == false)
                {
                    _discoveryTimer?.Change(
                        TimeSpan.Zero, Timeout.InfiniteTimeSpan);

                    _timerRanFirstTime = true;
                }
                else
                {
                    _discoveryTimer?.Change(
                        TimeSpan.FromMilliseconds((int)HostInfo.NetworkClass), Timeout.InfiniteTimeSpan);
                }
            }
            else
                _discoveryTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    private void InitOrToggleAliveTimer(bool state)
    {
        if (_aliveTimer == null)
        {
            _aliveTimer = new Timer(
                AliveTimerOnElapsed,
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
        }
        else if (state)
        {
            _aliveTimer.Change(
                TimeSpan.Zero,
                TimeSpan.FromSeconds(30));
        }
        else
        {
            _aliveTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    #endregion

    #region Internal

    private void DiscoveryTimerOnElapsed(object? stateInfo)
    {
        // Stop the timer until the probing packets are sent
        InitOrToggleDiscoveryTimer(false);

        ProbeDevices();

        // Resume the timer
        InitOrToggleDiscoveryTimer(true);
    }

    private void StartMonitoring()
    {
        // Start receiving packets
        ReceivePackets();

        // Setup the discovery timer
        InitOrToggleDiscoveryTimer(true);

        // Setup the device timout timer
        InitOrToggleAliveTimer(true);

        _logger!.Information("Service of type: {Type}, started",
            typeof(IDeviceScanner));
    }

    private void ReceivePackets()
    {
        if (_packetReceiveTechnique == PacketReceiveTechnique.EventHandler)
        {
            _device!.OnPacketArrival += (_, e) =>
            {
                if (_cancellationTokenSource?.IsCancellationRequested == false)
                    ProcessPacket(e);
            };

            _device?.StartCapture();
        }
        else
        {
            Task.Run(() =>
            {
                while (_cancellationTokenSource?.IsCancellationRequested == false)
                {
                    var packetResult = _device!.GetNextPacket(out var e);

                    if (packetResult != GetPacketStatus.PacketRead)
                        continue;

                    ProcessPacket(e);
                }
            }, _cancellationTokenSource!.Token);
        }
    }

    private void ProbeDevices()
    {
        switch (HostInfo.NetworkClass)
        {
            case NetworkClass.A:
                for (var i = 1; i <= 255; i++)
                for (var j = 1; j <= 255; j++)
                for (var k = 1; k <= 255; k++)
                    _device.SendPacket(BuildArpPacket(IPAddress.Parse(HostInfo.RootIp + i + '.' + j + '.' + k)));
                break;
            case NetworkClass.B:
                for (var i = 1; i <= 255; i++)
                for (var j = 1; j <= 255; j++)
                    _device.SendPacket(BuildArpPacket(IPAddress.Parse(HostInfo.RootIp + i + '.' + j)));
                break;
            case NetworkClass.C:
                for (var i = 1; i <= 255; i++)
                    _device.SendPacket(BuildArpPacket(IPAddress.Parse(HostInfo.RootIp + i)));
                break;
            case NetworkClass.D:
                throw new NotImplementedException("The detected network class is not supported.");
            case NetworkClass.E:
                throw new NotImplementedException("The detected network class is not supported.");
            default:
                throw new InvalidOperationException("The detected network class is invalid.");
        }
    }

    private void ProcessPacket(PacketCapture packetCapture)
    {
        var rawCapture = packetCapture.GetPacket();
        var packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
        var arpPacket = packet.Extract<ArpPacket>();

        if (arpPacket == null)
            return;

        var client = _clients.Lookup(arpPacket.SenderHardwareAddress.ToString());

        if (client.HasValue == false)
        {
            _clients.AddOrUpdate(new Device(arpPacket.SenderProtocolAddress, arpPacket.SenderHardwareAddress));

            var presentClient = _clients.Lookup(arpPacket.SenderHardwareAddress.ToString());

            // Get hostname for current target
            // We fire and forget since failing to resolve the hostname won't affect
            // the functionality
            _deviceNameResolver?.GetDeviceNameAsync(presentClient.Value);

            // Get vendor info for current target if the feature is available
            if (OptionalFeatures.AvailableFeatures.Contains(typeof(IDeviceTypeIdentifier)))
                _deviceTypeIdentifier?.IdentifyDeviceAsync(presentClient.Value);
        }
        else
        {
            client.Value.UpdateLastArpTime();
        }
    }

    private EthernetPacket BuildArpPacket(IPAddress targetIpAddress)
    {
        var arpRequestpacket = new ArpPacket(ArpOperation.Request,
            targetHardwareAddress: HostInfo.EmptyMac,
            targetProtocolAddress: targetIpAddress,
            senderHardwareAddress: HostInfo.HostMac,
            senderProtocolAddress: HostInfo.HostIp);

        var ethernetpacket = new EthernetPacket(
            sourceHardwareAddress: HostInfo.HostMac,
            destinationHardwareAddress: HostInfo.BroadcastMac,
            EthernetType.Arp);

        ethernetpacket.PayloadPacket = arpRequestpacket;

        return ethernetpacket;
    }

    private void AliveTimerOnElapsed(object? stateInfo)
    {
        foreach (var client in _clients.Items)
        {
            if (client.IsGateway() == false &&
                client.IsLocalDevice() == false &&
                DateTime.Now.Second - client.TimeSinceLastArp.Second > 30)
            {
                _clients.Remove(client);
            }
        }
    }

    #endregion

    #region API

    public void Scan()
    {
        if (IsStarted == false)
        {
            StartMonitoring();
            IsStarted = true;
        }

        _logger!.Information("Service of type: {Type}, started",
            typeof(IDeviceScanner));
    }

    public void Refresh()
    {
        ProbeDevices();
    }

    public void Stop()
    {
        InitOrToggleDiscoveryTimer(false);
        InitOrToggleAliveTimer(false);

        _cancellationTokenSource?.Cancel();
        IsStarted = false;
        _device?.StopCapture();

        _logger!.Information("Service of type: {Type}, stopped",
            typeof(IDeviceScanner));
    }

    public void Dispose()
    {
        Stop();

        if (_device != null)
        {
            _discoveryTimer?.Dispose();
            _aliveTimer?.Dispose();

            _device.Close();
            _device.Dispose();
            _device = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        _logger!.Information("Service of type: {Type}, disposed",
            typeof(IDeviceScanner));
    }

    #endregion
}