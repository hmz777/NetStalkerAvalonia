using DynamicData;
using DynamicData.Binding;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Models;
using PacketDotNet;
using ReactiveUI;
using Serilog;
using SharpPcap;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Services.Implementations.BlockingRedirection
{
	public class BlockerRedirector : IBlockerRedirector
	{
		#region Subscriptions

		private IDisposable? _deviceListener;
		private IDisposable? _clientsToRules;
		private IDisposable? _ruleUpdates;

		#endregion

		#region Members

		private Task? serviceTask;
		private CancellationTokenSource? _cancellationTokenSource;
		private bool _isStarted;
		private IPcapLiveDevice _device;
		private Timer? _byteCounterTimer;

		private readonly IRuleService _ruleService;
		private readonly IPcapDeviceManager _pcapDeviceManager;
		private readonly IMessageBusService _messageBusService;

		// Collection projected from the scanner via the message bus
		private ReadOnlyObservableCollection<Device> _clients = new(new ObservableCollection<Device>());

		#endregion

		#region Constructor

		public BlockerRedirector(
			IRuleService ruleService,
			IPcapDeviceManager pcapDeviceManager,
			IMessageBusService messageBusService)
		{
			_ruleService = ruleService;
			_pcapDeviceManager = pcapDeviceManager;
			_messageBusService = messageBusService;

			InitDevice();
			ListenToTheScanner();
			BindClientsToRules();
			SubscribeToRuleUpdates();
			StartIfNotStarted();

			Log.Information(LogMessageTemplates.ServiceInit,
				typeof(IBlockerRedirector));
		}

		#endregion

		#region Init

		private void InitDevice()
		{
			if (_device == null)
			{
				_device = _pcapDeviceManager.CreateDevice("ip", DeviceOnPacketArrival, 1000);

				// The state parameter here doesn't matter since we're initializing the timer
				// it will be later started when this service is started
				InitOrToggleByteCounterTimer(true);
			}
		}

		private void ListenToTheScanner()
		{
			// Subscribe to the scanner device stream

			//_deviceListener = MessageBus
			//		.Current
			//		.Listen<IChangeSet<Device, string>>(ContractKeys.ScannerStream.ToString())
			//		.ObserveOn(RxApp.MainThreadScheduler)
			//		.Bind(out _clients)
			//		.DisposeMany()
			//		.Subscribe();

			_deviceListener = _messageBusService.Listen<IChangeSet<Device, string>>(observable =>
			{
				var disposable = observable.ObserveOn(RxApp.MainThreadScheduler)
						.Bind(out _clients)
						.DisposeMany()
						.Subscribe();

				return disposable;

			}, ContractKeys.ScannerStream.ToString());
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
					TimeSpan.FromSeconds(2));
			}
			else
			{
				_byteCounterTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			}
		}

		private void SubscribeToRuleUpdates()
		{
			_ruleUpdates = _ruleService.Rules
					.ToObservableChangeSet()
					.AutoRefresh()
					.DisposeMany()
					.Subscribe(changeSet =>
					{
						foreach (var client in _clients)
						{
							if (client.IsGateway() == false && client.IsLocalDevice() == false)
							{
								_ruleService.ApplyIfMatch(client);
							}
						}
					});
		}

		private void BindClientsToRules()
		{
			_clientsToRules = _clients
			   .ToObservableChangeSet()
			   .ObserveOn(RxApp.MainThreadScheduler)
			   .DisposeMany()
			   .Where(x => x.Adds > 0)
			   .ToCollection()
			   .Select(clients => clients.OrderBy(x => x.DateAdded).LastOrDefault())
			   .Where(client => client != null)
			   .Subscribe(client =>
			   {
				   if (client!.IsGateway() == false && client.IsLocalDevice() == false)
				   {
					   _ruleService.ApplyIfMatch(client);
				   }
			   });
		}

		#endregion

		#region Internal

		private void ByteCounterTimerOnElapsed(object? stateInfo)
		{
			var targets = _clients.Where(c => c.Redirected == true).ToList();

			foreach (var client in targets)
			{
				client?.ResetSentBytes();
				client?.ResetReceivedBytes();
			}
		}

		private void DeviceOnPacketArrival(object sender, PacketCapture e)
		{
			if (_isStarted == false)
				return;

			var rawCapture = e.GetPacket();

			if (Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data) is not EthernetPacket packet)
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
			try
			{
				if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
				{
					_cancellationTokenSource = new CancellationTokenSource();
				}
				else
				{
					if (_cancellationTokenSource.TryReset() == false)
						throw new InvalidOperationException("Can not reset the Blocker/Redirector cancellation token.");
				}

				if (_isStarted == false)
				{
					if (_device is not null && _device is { Opened: true, Started: false })
					{
						_device.StartCapture();

						serviceTask = Task.Run(async () =>
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

					Log.Information(LogMessageTemplates.ServiceStart,
									typeof(IBlockerRedirector));
				}
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), this.GetType(), e.Message);
			}
		}

		private void SpoofClients()
		{
			foreach (var client in _clients)
			{
				if ((client.Blocked || client.Redirected)
					&& client.IsLocalDevice() == false
					&& client.IsGateway() == false)
				{
					try
					{
						ConstructAndSendArp(client, ArpPacketType.Spoof);

						if (Config.AppSettings!.SpoofProtectionSetting)
							ConstructAndSendArp(client, ArpPacketType.Protection);
					}
					catch (Exception e)
					{
						Log.Error(LogMessageTemplates.ExceptionTemplate,
							e.GetType(), this.GetType(), e.Message);
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
					//MaintainTarget();
					MaintainHost();
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

			void MaintainHost()
			{
				var arpPacketForHostProtection = new ArpPacket(ArpOperation.Response,
					targetHardwareAddress: HostInfo.GatewayMac,
					targetProtocolAddress: HostInfo.GatewayIp,
					senderHardwareAddress: HostInfo.HostMac,
					senderProtocolAddress: HostInfo.HostIp);

				var etherPacketForHostProtection = new EthernetPacket(
					sourceHardwareAddress: HostInfo.HostMac,
					destinationHardwareAddress: HostInfo.GatewayMac,
					EthernetType.Arp)
				{
					PayloadPacket = arpPacketForHostProtection
				};

				_device.SendPacket(etherPacketForHostProtection);
			}
		}

		private void TryPauseIfNoDevicesLeft()
		{
			if (_clients is not null && _clients.Any(client => client.Blocked || client.Redirected) == false)
			{
				// If no clients have active blocking or redirection we pause the service
				// so we don't do extra work on idle
				Stop();
			}
		}

		private void Stop()
		{
			if (_isStarted)
			{
				_cancellationTokenSource?.Cancel();
				serviceTask?.Wait();
				serviceTask?.Dispose();
				InitOrToggleByteCounterTimer(false);
				_isStarted = false;

				Log.Information(LogMessageTemplates.ServiceStop,
					typeof(IBlockerRedirector));
			}
		}

		#endregion

		#region API

		public bool Status => _isStarted;

		public ReadOnlyObservableCollection<Device> Devices => _clients!;

		public void Block(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			if (device.Redirected)
				device.UnRedirect();

			device.Block();

			Log.Information(LogMessageTemplates.DeviceBlock,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip);
		}

		public void Redirect(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			if (device.Blocked)
				device.UnBlock();

			device.Redirect();

			Log.Information(LogMessageTemplates.DeviceRedirect,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip);
		}

		public void UnBlock(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			device.UnBlock();
			//TryPauseIfNoDevicesLeft();

			Log.Information(LogMessageTemplates.DeviceUnblock,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip);
		}

		public void UnRedirect(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			device.UnRedirect();
			//TryPauseIfNoDevicesLeft();

			Log.Information(LogMessageTemplates.DeviceUnRedirect,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip);
		}

		public void Limit(Device device, int download, int upload)
		{
			if (download < 0 || upload < 0)
			{
				throw new InvalidOperationException("Download and Upload limits can't be negative");
			}

			Redirect(device);

			device.SetDownloadCap(download);
			device.SetUploadCap(upload);

			Log.Information(LogMessageTemplates.DeviceLimit,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip,
				device.DownloadCap,
				device.UploadCap);
		}

		public void Dispose()
		{
			Stop();

			if (_device != null)
			{
				_byteCounterTimer?.Dispose();
				_device.Dispose(DeviceOnPacketArrival);
				_device = null;
				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}

			_deviceListener?.Dispose();
			_clientsToRules?.Dispose();
			_ruleUpdates?.Dispose();

			Log.Information(LogMessageTemplates.ServiceDispose,
				typeof(IBlockerRedirector));
		}

		#endregion
	}
}