using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using NetStalkerAvalonia.Services;
using ReactiveUI;

namespace NetStalkerAvalonia.Models
{
	public class Device : ReactiveObject
	{
		public Device(IPAddress ip, PhysicalAddress mac)
		{
			ArgumentNullException.ThrowIfNull(ip, nameof(ip));
			ArgumentNullException.ThrowIfNull(mac, nameof(mac));

			Ip = ip;
			Mac = mac;
			Name = "Resolving...";
			Type = DeviceType.PC;
			DateAdded = DateTime.Now;

			_isResolving = this.WhenAnyValue(x => x.Name)
				.Select(state => state == "Resolving...")
				.ToProperty(this, x => x.IsResolving);

			_uploadSpeed = this.WhenAnyValue(x => x.BytesSentSinceLastReset)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Select(bytes => Math.Round(bytes / 1024f, 2))
				.ObserveOn(RxApp.MainThreadScheduler)
				.ToProperty(this, x => x.UploadSpeed);

			_downloadSpeed = this.WhenAnyValue(x => x.BytesReceivedSinceLastReset)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Select(bytes => Math.Round(bytes / 1024f, 2))
				.ObserveOn(RxApp.MainThreadScheduler)
				.ToProperty(this, x => x.DownloadSpeed);
		}

		#region Properties

		public IPAddress Ip { get; }
		public PhysicalAddress Mac { get; }
		public DateTime DateAdded { get; }
		public bool HasFriendlyName { get; private set; }

		#endregion

		#region Reactive Properties

		private bool _blocked;

		public bool Blocked
		{
			get => _blocked;
			private set => this.RaiseAndSetIfChanged(ref _blocked, value);
		}

		private bool _redirected;

		public bool Redirected
		{
			get => _redirected;
			private set => this.RaiseAndSetIfChanged(ref _redirected, value);
		}

		private int _downloadCap;

		public int DownloadCap
		{
			get => _downloadCap;
			private set => this.RaiseAndSetIfChanged(ref _downloadCap, value);
		}

		private int _uploadCap;

		public int UploadCap
		{
			get => _uploadCap;
			private set => this.RaiseAndSetIfChanged(ref _uploadCap, value);
		}

		private string? _name;

		public string? Name
		{
			get => _name;
			private set => this.RaiseAndSetIfChanged(ref _name, value);
		}

		private DateTime _timeSinceLastArp;

		public DateTime TimeSinceLastArp
		{
			get => _timeSinceLastArp;
			private set => this.RaiseAndSetIfChanged(ref _timeSinceLastArp, value);
		}

		private long _bytesSentSinceLastReset;

		public long BytesSentSinceLastReset
		{
			get => _bytesSentSinceLastReset;
			private set => this.RaiseAndSetIfChanged(ref _bytesSentSinceLastReset, value);
		}

		private long _bytesReceivedSinceLastReset;

		public long BytesReceivedSinceLastReset
		{
			get => _bytesReceivedSinceLastReset;
			private set => this.RaiseAndSetIfChanged(ref _bytesReceivedSinceLastReset, value);
		}

		private string? _vendor;
		public string? Vendor
		{
			get => _vendor;
			private set => this.RaiseAndSetIfChanged(ref _vendor, value);
		}

		private DeviceType _type;
		public DeviceType Type
		{
			get => _type;
			private set => this.RaiseAndSetIfChanged(ref _type, value);
		}

		private readonly ObservableAsPropertyHelper<bool> _isResolving;
		public bool IsResolving => _isResolving.Value;

		private readonly ObservableAsPropertyHelper<double> _downloadSpeed;
		public double DownloadSpeed => _downloadSpeed.Value;

		private readonly ObservableAsPropertyHelper<double> _uploadSpeed;
		public double UploadSpeed => _uploadSpeed.Value;

		#endregion

		#region Methods

		public void SetFriendlyName(string? name, bool isResolvedHostName = false)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				Name = Ip.ToString();
				HasFriendlyName = false;
			}
			else
			{
				Name = name;
				HasFriendlyName = !isResolvedHostName;
			}
		}

		public void ClearFriendlyName()
		{
			Name = null;
			HasFriendlyName = false;
		}

		public void SetVendor(string vendor) => Vendor = vendor;
		public void Block() => Blocked = true;
		public void UnBlock() => Blocked = false;
		public void Redirect() => Redirected = true;
		public void UnRedirect() => Redirected = false;
		public void SetDownloadCap(int downloadCap) => DownloadCap = downloadCap * 1024;
		public void SetUploadCap(int uploadCap) => UploadCap = uploadCap * 1024;
		public void IncrementSentBytes(long bytes) => BytesSentSinceLastReset += bytes;
		public void IncrementReceivedBytes(long bytes) => BytesReceivedSinceLastReset += bytes;
		public void ResetSentBytes() => BytesSentSinceLastReset = 0;
		public void ResetReceivedBytes() => BytesReceivedSinceLastReset = 0;
		public void UpdateLastArpTime() => TimeSinceLastArp = DateTime.Now;

		public bool IsGateway() => Mac!.Equals(HostInfo.GatewayMac);
		public bool IsLocalDevice() => Mac!.Equals(HostInfo.HostMac);

		public void ResetState()
		{
			UnBlock();
			UnRedirect();
			SetDownloadCap(0);
			SetUploadCap(0);
		}

		#endregion
	}
}