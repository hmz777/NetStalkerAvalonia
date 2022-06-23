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
        }

        #region Properties

        public IPAddress? Ip { get; set; }
        public PhysicalAddress? Mac { get; private set; }
        public int Download { get; private set; }
        public int Upload { get; private set; }
        public string? Vendor { get; private set; }
        public DeviceType Type { get; private set; }
        public DateTime DateAdded { get; }
        public long BytesSentSinceLastReset { get; private set; }
        public long BytesReceivedSinceLastReset { get; private set; }
        public DateTime TimeSinceLastArp { get; private set; }

        #endregion
        
        #region Reactive Properties

        private bool _blocked;

        public bool Blocked
        {
            get => _blocked;
            set => this.RaiseAndSetIfChanged(ref _blocked, value);
        }
        
        private bool _redirected;

        public bool Redirected
        {
            get => _redirected;
            set => this.RaiseAndSetIfChanged(ref _redirected, value);
        }
        
        private string? _name;

        public string? Name
        {
            get => _name;
            private set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        
        private readonly ObservableAsPropertyHelper<bool> _isResolving;
        public bool IsResolving => _isResolving.Value;

        #endregion

        #region Methods

        public void SetFriendlyName(string name) => Name = name;
        public void SetVendor(string vendor) => Vendor = vendor;
        public void Block() => Blocked = true;
        public void UnBlock() => Blocked = false;
        public void Redirect() => Redirected = true;
        public void UnRedirect() => Redirected = false;
        public void SetDownload(int download) => Download = download * 1024;
        public void SetUpload(int upload) => Upload = upload * 1024;
        public void SetSentBytes(long bytes) => BytesSentSinceLastReset += bytes;
        public void SetReceivedBytes(long bytes) => BytesReceivedSinceLastReset += bytes;
        public void UpdateLastArpTime() => TimeSinceLastArp = DateTime.Now;

        public bool IsGateway() => Mac!.Equals(HostInfo.GatewayMac);
        public bool IsLocalDevice() => Mac!.Equals(HostInfo.HostMac);

        #endregion
    }
}