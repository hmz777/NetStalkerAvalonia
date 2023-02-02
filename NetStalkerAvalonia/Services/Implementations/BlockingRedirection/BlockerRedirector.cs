using DynamicData;
using DynamicData.Binding;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.ViewModels;
using PacketDotNet;
using Serilog;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Services.Implementations.BlockingRedirection
{
	public class BlockerRedirector : IBlockerRedirector
	{
		#region Subscriptions

		IDisposable? clientsToRules;
		IDisposable? ruleUpdates;

		#endregion

		#region Members

		private CancellationTokenSource? _cancellationTokenSource;
		private bool _isStarted;
		private LibPcapLiveDevice? _device;
		private Timer? _byteCounterTimer;

		private readonly IRuleService ruleService;

		private static readonly ReadOnlyObservableCollection<Device> Clients = MainWindowViewModel.Devices;

		#endregion

		#region Constructor

		public BlockerRedirector(IRuleService ruleService = null!)
		{
			this.ruleService = Tools.ResolveIfNull(ruleService);

			InitDevice();
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

		private void SubscribeToRuleUpdates()
		{
			ruleUpdates = ruleService.Rules
					.ToObservableChangeSet()
					.AutoRefresh()
					.DisposeMany()
					.Subscribe(changeSet =>
					{
						foreach (var client in Clients)
						{
							if (client.IsGateway() == false && client.IsLocalDevice() == false)
							{
								ruleService.ApplyIfMatch(client);
							}
						}
					});
		}

		private void BindClientsToRules()
		{
			clientsToRules = Clients
				   .ToObservableChangeSet()
				   .DisposeMany()
				   .Where(x => x.Adds > 0)
				   .ToCollection()
				   .Select(client => client.LastOrDefault())
				   .Where(client => client != null)
				   .Subscribe(client =>
				   {
					   if (client!.IsGateway() == false && client.IsLocalDevice() == false)
					   {
						   ruleService.ApplyIfMatch(client);
					   }
				   });
		}

		#endregion

		#region Internal

		private void ByteCounterTimerOnElapsed(object? stateInfo)
		{
			if (Clients != null)
			{
				foreach (var client in Clients)
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

			var outTarget = Clients?
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
				var inTarget = Clients?
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
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), this.GetType(), e.Message);
			}

			Log.Information(LogMessageTemplates.ServiceStart,
				typeof(IBlockerRedirector));
		}

		private void SpoofClients()
		{
			foreach (var client in Clients!)
			{
				if ((client.Blocked || client.Redirected)
					&& client.IsLocalDevice() == false
					&& client.IsGateway() == false)
				{
					ConstructAndSendArp(client, ArpPacketType.Spoof);

					try
					{
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
			if ((bool)Clients?.Any(client => client.Blocked || client.Redirected) == false)
			{
				// If no clients have active blocking or redirection we pause the service 
				// so we don't do extra work on idle
				Stop();

				Log.Information(LogMessageTemplates.ServiceStop,
					typeof(IBlockerRedirector));
			}
		}

		private void Stop()
		{
			_cancellationTokenSource?.Cancel();
			InitOrToggleByteCounterTimer(false);
			_isStarted = false;
			_device?.StopCapture();

			Log.Information(LogMessageTemplates.ServiceStop,
				typeof(IBlockerRedirector));
		}

		#endregion

		#region API

		public bool Status => _isStarted;

		public void Block(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			if (device.Redirected)
				device.UnRedirect();

			device.Block();
			StartIfNotStarted();

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
			StartIfNotStarted();

			Log.Information(LogMessageTemplates.DeviceRedirect,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip);
		}

		public void UnBlock(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			device.UnBlock();
			TryPauseIfNoDevicesLeft();

			Log.Information(LogMessageTemplates.DeviceUnblock,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip);
		}

		public void UnRedirect(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			device.UnRedirect();
			TryPauseIfNoDevicesLeft();

			Log.Information(LogMessageTemplates.DeviceUnRedirect,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip);
		}

		public void Limit(Device device, int download, int upload)
		{
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

		public void LimitDownload(Device device, int download)
		{
			Redirect(device);

			device.SetDownloadCap(download);

			Log.Information(LogMessageTemplates.DeviceLimit,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip,
				device.DownloadCap,
				device.UploadCap);
		}

		public void LimitUpload(Device device, int upload)
		{
			Redirect(device);

			device.SetUploadCap(upload);

			Log.Information(LogMessageTemplates.DeviceLimit,
				typeof(IBlockerRedirector),
				device.Mac,
				device.Ip,
				device.DownloadCap,
				device.UploadCap);
		}

		public void ClearLimits(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			device.SetDownloadCap(0);
			device.SetUploadCap(0);

			Log.Information(LogMessageTemplates.DeviceLimitsClear,
				typeof(IBlockerRedirector), device.Mac);
		}

		public void ClearDownload(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			device.SetDownloadCap(0);

			Log.Information(LogMessageTemplates.DeviceDownloadLimitClear,
				typeof(IBlockerRedirector), device.Mac);
		}

		public void ClearUpload(Device device)
		{
			ArgumentNullException.ThrowIfNull(device, nameof(device));

			device.SetUploadCap(0);

			Log.Information(LogMessageTemplates.DeviceUploadLimitClear,
				typeof(IBlockerRedirector), device.Mac);
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

			clientsToRules?.Dispose();
			ruleUpdates?.Dispose();

			Log.Information(LogMessageTemplates.ServiceDispose,
				typeof(IBlockerRedirector));
		}

		#endregion
	}
}