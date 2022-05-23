using System;
using System.Net;
using System.Threading;
using DynamicData;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using PacketDotNet;
using ReactiveUI;
using SharpPcap;
using SharpPcap.LibPcap;
using Timer = System.Threading.Timer;

namespace NetStalkerAvalonia.Services.Implementations.DeviceScanning;

public class DeviceScanner : IDeviceScanner
{
    #region Members

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isStarted;
    private LibPcapLiveDevice? _device;
    private Timer? _discoveryTimer;

    private IDeviceNameResolver? _deviceNameResolver;
    private IDeviceTypeIdentifier? _deviceTypeIdentifier;

    // This is the original source of clients and all other collections are projections from this
    private SourceCache<Device, string> _clients = new(x => x.Mac!.ToString());

    #endregion

    #region Constructor

    public DeviceScanner(
        IDeviceNameResolver deviceNameResolver,
        IDeviceTypeIdentifier deviceTypeIdentifier)
    {
        _deviceNameResolver = Tools.ResolveIfNull(deviceNameResolver);
        _deviceTypeIdentifier = Tools.ResolveIfNull(deviceTypeIdentifier);

        Init();
        SetupBindings();
    }

    #endregion

    #region Init

    private void Init()
    {
        if (_device == null)
        {
            _device = (LibPcapLiveDevice)CaptureDeviceList.New()[HostInfo.NetworkAdapterName];
            _device.Open(DeviceModes.Promiscuous, 1000);
            _device.Filter = "arp";

            // The state parameter here doesn't matter since we're initializing the timer
            // it will be later started when the scan functionality is started
            InitOrToggleDiscoveryTimer(true);

            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void SetupBindings() => MessageBus.Current.RegisterMessageSource(_clients.Connect());

    private void InitOrToggleDiscoveryTimer(bool state)
    {
        if (_discoveryTimer == null)
        {
            _discoveryTimer = new Timer(DiscoveryTimerOnElapsed, null,
                Timeout.InfiniteTimeSpan,
                TimeSpan.FromMilliseconds((int)HostInfo.NetworkClass));
        }
        else
        {
            if (state)
                _discoveryTimer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds((int)HostInfo.NetworkClass));
            else
                _discoveryTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
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
        _device!.OnPacketArrival += (object sender, PacketCapture e) =>
        {
            if (_cancellationTokenSource?.IsCancellationRequested == false)
                ProcessPacket(e);
        };

        // Setup the discovery timer
        InitOrToggleDiscoveryTimer(true);

        // Start receiving packets
        _device.StartCapture();
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
        var rawcapture = packetCapture.GetPacket();
        var packet = Packet.ParsePacket(rawcapture.LinkLayerType, rawcapture.Data);
        var arpPacket = packet.Extract<ArpPacket>();

        if (arpPacket == null)
            return;

        var client = _clients.Lookup(arpPacket.SenderHardwareAddress.ToString());

        if (client.HasValue == false)
        {
            _clients.AddOrUpdate(new Device(arpPacket.SenderProtocolAddress, arpPacket.SenderHardwareAddress));

            // Get hostname for current target
            _deviceNameResolver?.GetDeviceName(client.Value);

            // Get vendor info for current target
            _deviceTypeIdentifier?.IdentifyDevice(client.Value);
        }
        else
        {
            client.Value.UpdateLastArpTime();
        }
    }

    private EthernetPacket BuildArpPacket(IPAddress targetIpAddress)
    {
        var arprequestpacket = new ArpPacket(ArpOperation.Request,
            targetHardwareAddress: HostInfo.EmptyMac,
            targetProtocolAddress: targetIpAddress,
            senderHardwareAddress: HostInfo.HostMac,
            senderProtocolAddress: HostInfo.HostIp);

        var ethernetpacket = new EthernetPacket(
            sourceHardwareAddress: HostInfo.HostMac,
            destinationHardwareAddress: HostInfo.BroadcastMac,
            EthernetType.Arp);

        ethernetpacket.PayloadPacket = arprequestpacket;

        return ethernetpacket;
    }

    #endregion

    #region API

    public void Scan()
    {
        if (_isStarted == false)
        {
            StartMonitoring();
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _isStarted = false;
        _device?.StopCapture();
    }

    public void Dispose()
    {
        Stop();

        if (_device != null)
        {
            _device.Close();
            _device.Dispose();
            _device = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    #endregion
}