using NetStalkerAvalonia.Models;
using System.Net;
using System.Net.NetworkInformation;
using NetStalkerAvalonia.Helpers;
using System;
using Splat;

namespace NetStalkerAvalonia.Services
{
	public static class HostInfo
	{
		#region Gateway

		public static IPAddress? GatewayIp { get; private set; }
		public static PhysicalAddress? GatewayMac { get; private set; }

		#endregion

		#region Host

		public static IPAddress? HostIp { get; private set; }
		public static PhysicalAddress? HostMac { get; private set; }

		#endregion

		#region Network

		public static IPAddress? SubnetMask { get; private set; }
		public static IpType IpType { get; private set; }
		public static NetworkClass NetworkClass { get; private set; }
		public static string? NetworkAdapterName { get; private set; }

		#endregion

		#region Tools

		public static void SetHostInfo(
			string networkAdapterName,
			IPAddress gatewayIp,
			PhysicalAddress gatewayMac,
			IPAddress hostIp,
			PhysicalAddress hostMac,
			IpType ipType,
			IPAddress subnetMask,
			NetworkClass networkClass)
		{
			try
			{
				ArgumentException.ThrowIfNullOrEmpty(networkAdapterName, nameof(networkAdapterName));
				ArgumentNullException.ThrowIfNull(gatewayIp, nameof(gatewayIp));
				ArgumentNullException.ThrowIfNull(gatewayMac, nameof(gatewayMac));
				ArgumentNullException.ThrowIfNull(hostIp, nameof(hostIp));
				ArgumentNullException.ThrowIfNull(hostMac, nameof(hostMac));

				if (ipType == IpType.Ipv4)
				{
					ArgumentNullException.ThrowIfNull(subnetMask, nameof(subnetMask));

					if (networkClass == NetworkClass.None)
					{
						throw new ArgumentException("Invalid network class");
					}
				}

				GatewayIp = gatewayIp;
				GatewayMac = gatewayMac;
				HostIp = hostIp;
				HostMac = hostMac;
				SubnetMask = subnetMask;
				IpType = ipType;
				NetworkClass = networkClass;
				NetworkAdapterName = networkAdapterName;
			}
			catch
			{
				if (ModeDetector.InUnitTestRunner())
				{
					GatewayIp = Tools.GetRandomIpAddress();
					GatewayMac = Tools.GetRandomMacAddress();
					HostIp = Tools.GetRandomIpAddress();
					HostMac = Tools.GetRandomMacAddress();
					IpType = ipType;

					if (IpType == IpType.Ipv4)
					{
						SubnetMask = IPAddress.Parse("255.255.255.0");
						NetworkClass = NetworkClass.C;
					}

					NetworkAdapterName = "Wi-Fi";

					return;
				}

				throw;
			}
		}

		public static string? RootIp => Tools.GetRootIp(HostIp!, NetworkClass);

		#endregion

		#region Constants

		public static PhysicalAddress BroadcastMac = PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF");
		public static PhysicalAddress EmptyMac = PhysicalAddress.Parse("00-00-00-00-00-00");

		#endregion
	}
}