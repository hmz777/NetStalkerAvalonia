using NetStalkerAvalonia.Models;
using System.Net;
using System.Net.NetworkInformation;

namespace NetStalkerAvalonia.Services
{
    public static class HostInfo
    {
        public static IPAddress? GatewayIp { get; set; }
        public static PhysicalAddress? GatewayMac { get; set; }

        public static IPAddress? HostIp { get; set; }
        public static PhysicalAddress? HostMac { get; set; }

        public static NetworkClass NetworkClass { get; set; }
        public static IPAddress? SubnetMask { get; set; }
        public static IPAddress? BroadcastIp { get; set; }
        public static PhysicalAddress? BroadcastMac { get; set; } = PhysicalAddress.Parse("FF-FF-FF-FF-FF");
        public static string? NetworkAdapterName { get; set; }
    }
}