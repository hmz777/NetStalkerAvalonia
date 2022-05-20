using System;
using System.Net;
using System.Net.NetworkInformation;
using NetStalkerAvalonia.Services;

namespace NetStalkerAvalonia.Models
{
    public class Device
    {
        public Device(IPAddress ip, PhysicalAddress mac)
        {
            ArgumentNullException.ThrowIfNull(ip, nameof(ip));
            ArgumentNullException.ThrowIfNull(mac, nameof(mac));

            IP = ip;
            Mac = mac;
            Name = IP?.ToString();
            Type = DeviceType.PC;
            DateAdded = DateTime.Now;
        }

        #region Properties

        public IPAddress? IP { get; private set; }
        public PhysicalAddress? Mac { get; private set; }
        public bool Blocked { get; private set; }
        public bool Redirected { get; private set; }
        public int Download { get; private set; }
        public int Upload { get; private set; }
        public string? Name { get; private set; }
        public DeviceType Type { get; private set; }
        public DateTime DateAdded { get; }
        public long BytesSentSinceLastReset { get; private set; }
        public long BytesReceivedSinceLastReset { get; private set; }

        #endregion

        #region Methods

        public void SetFriendlyName(string name) => Name = name;
        public void Block() => Blocked = true;
        public void UnBlock() => Blocked = false;
        public void Redirect() => Redirected = true;
        public void UnRedirect() => Redirected = false;
        public void SetDownload(int download) => Download = download;
        public void SetUpload(int upload) => Upload = upload;
        public void SetSentBytes(long bytes) => BytesSentSinceLastReset += bytes;
        public void SetReceivedBytes(long bytes) => BytesReceivedSinceLastReset += bytes;

        public bool IsGateway() => Mac.Equals(HostInfo.GatewayMac);
        public bool IsLocalDevice() => Mac.Equals(HostInfo.HostMac);

        #endregion
    }
}