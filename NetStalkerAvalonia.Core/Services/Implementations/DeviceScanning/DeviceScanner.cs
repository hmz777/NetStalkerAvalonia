using DynamicData;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Models;
using PacketDotNet;
using Serilog;
using SharpPcap;
using System;
using System.Net;
using System.Threading;
using Timer = System.Threading.Timer;

namespace NetStalkerAvalonia.Core.Services.Implementations.DeviceScanning;

public class DeviceScanner : IDeviceScanner
{
	#region Members

	IDisposable? _deviceMessageSource;

	private CancellationTokenSource? _cancellationTokenSource;
	private IPcapLiveDevice _device;
	private Timer? _discoveryTimer;
	private Timer? _aliveTimer;
	private bool _timerRanFirstTime;
	private bool _isStarted;

	// Services
	private readonly IPcapDeviceManager _pcapDeviceManager;
	private readonly IDeviceNameResolver _deviceNameResolver;
	private readonly IDeviceTypeIdentifier _deviceTypeIdentifier;
	private readonly IMessageBusService _messageBusService;

	// This is the original source of clients and all other collections are projections from this
	private SourceCache<Device, string> _clients = new(x => x.Mac!.ToString());

	#endregion

	#region Constructor

	public DeviceScanner(
		IPcapDeviceManager pcapDeviceManager,
		IDeviceNameResolver deviceNameResolver,
		IDeviceTypeIdentifier deviceTypeIdentifier,
		IMessageBusService messageBusService)
	{
		_pcapDeviceManager = pcapDeviceManager;
		_deviceNameResolver = deviceNameResolver;
		_deviceTypeIdentifier = deviceTypeIdentifier;
		_messageBusService = messageBusService;

		Init();
		SetupBindings();

		Log.Information(LogMessageTemplates.ServiceInit,
			typeof(IDeviceScanner));
	}

	#endregion

	#region Init

	private void Init()
	{
		if (_device == null)
		{
			_device = _pcapDeviceManager.CreateDevice("arp", OnPacketArrival, 20);

			// The state parameter here doesn't matter since we're initializing the timer
			// it will be later started when the scan functionality is started
			InitOrToggleDiscoveryTimer(true);
			InitOrToggleAliveTimer(true);

			_cancellationTokenSource = new CancellationTokenSource();
		}
	}

	private void SetupBindings()
	{
		_deviceMessageSource = _messageBusService.RegisterMessageSource(_clients.Connect(), ContractKeys.ScannerStream.ToString());
	}

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
	}

	private void OnPacketArrival(object sender, PacketCapture packetCapture)
	{
		if (_cancellationTokenSource?.IsCancellationRequested == false)
			ProcessPacket(packetCapture);
	}

	private void ReceivePackets()
	{
		_device.StartCapture();
	}

	private void ProbeDevices()
	{
		// TODO: Remove the network class concept

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
			var device = presentClient.Value;

			// Get hostname for current target
			// We fire and forget since failing to resolve the hostname won't affect
			// the functionality
			_deviceNameResolver?.ResolveDeviceNameAsync(device);

			// Get vendor info for current target if the feature is available
			_deviceTypeIdentifier?.IdentifyDevice(device);
		}
		else
		{
			client.Value.UpdateLastArpTime();
		}
	}

	private EthernetPacket BuildArpPacket(IPAddress targetIpAddress)
	{
		var arpRequestPacket = new ArpPacket(ArpOperation.Request,
			targetHardwareAddress: HostInfo.EmptyMac,
			targetProtocolAddress: targetIpAddress,
			senderHardwareAddress: HostInfo.HostMac,
			senderProtocolAddress: HostInfo.HostIp);

		var ethernetPacket = new EthernetPacket(
			sourceHardwareAddress: HostInfo.HostMac,
			destinationHardwareAddress: HostInfo.BroadcastMac,
			EthernetType.Arp);

		ethernetPacket.PayloadPacket = arpRequestPacket;

		return ethernetPacket;
	}

	private void AliveTimerOnElapsed(object? stateInfo)
	{
		foreach (var client in _clients.Items)
		{
			if (client.IsGateway() == false &&
				client.IsLocalDevice() == false &&
				(DateTime.Now - client.TimeSinceLastArp).Seconds > 30)
			{
				_clients.Remove(client);
			}
		}
	}

	#endregion

	#region API

	public bool Status => _isStarted;

	public void Scan()
	{
		if (_isStarted == false)
		{
			StartMonitoring();
			_isStarted = true;

			Log.Information(LogMessageTemplates.ServiceStart,
				typeof(IDeviceScanner));
		}
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
		_isStarted = false;

		Log.Information(LogMessageTemplates.ServiceStop,
			typeof(IDeviceScanner));
	}

	public void Dispose()
	{
		Stop();

		if (_device != null)
		{
			_deviceMessageSource?.Dispose();
			_discoveryTimer?.Dispose();
			_aliveTimer?.Dispose();
			_device.Dispose(OnPacketArrival);
			_device = null;
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
		}

		Log.Information(LogMessageTemplates.ServiceDispose,
			typeof(IDeviceScanner));
	}

	#endregion
}