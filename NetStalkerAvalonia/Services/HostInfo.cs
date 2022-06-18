using NetStalkerAvalonia.Models;
using System.Net;
using System.Net.NetworkInformation;
using NetStalkerAvalonia.Helpers;

namespace NetStalkerAvalonia.Services
{
    public static class HostInfo
    {
        #region Gateway

        public static IPAddress? GatewayIp { get; set; }
        public static PhysicalAddress? GatewayMac { get; set; }

        #endregion

        #region Host

        public static IPAddress? HostIp { get; set; }
        public static PhysicalAddress? HostMac { get; set; }

        #endregion

        #region Network

        public static IpType IpType { get; set; }
        public static NetworkClass NetworkClass { get; set; }
        public static IPAddress? SubnetMask { get; set; }
        public static IPAddress? BroadcastIp { get; set; }
        public static PhysicalAddress? BroadcastMac = PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF");
        public static string? NetworkAdapterName { get; set; }

        #endregion

        public static string? RootIp => Tools.GetRootIp(HostIp!, NetworkClass);
        public static PhysicalAddress? EmptyMac = PhysicalAddress.Parse("00-00-00-00-00-00");
    }
}