using NetStalkerAvalonia.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace NetStalkerAvalonia.Core.Helpers
{
	public class DataHelpers
	{
		public static string GetRootIp(IPAddress ipaddress, NetworkClass networkClass)
		{
			ArgumentNullException.ThrowIfNull(ipaddress);

			var ipaddressstring = ipaddress.ToString();

			switch (networkClass)
			{
				case NetworkClass.A:
					return ipaddressstring
						.Substring(0,
							ipaddressstring
								.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1);
				case NetworkClass.B:
					return ipaddressstring
						.Substring(0,
							ipaddressstring
								.IndexOf(".", 4, StringComparison.InvariantCultureIgnoreCase) + 1);
				case NetworkClass.C:
					return ipaddressstring
						.Substring(0,
							ipaddressstring
								.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1);
				default:
					throw new ArgumentOutOfRangeException(nameof(networkClass), networkClass, null);
			}
		}

		public static PhysicalAddress GetRandomMacAddress()
		{
			var buffer = new byte[6];
			new Random().NextBytes(buffer);
			var result = string.Concat(buffer.Select(x => string.Format("{0}:", x.ToString("X2"))).ToArray());
			return PhysicalAddress.Parse(result.TrimEnd(':'));
		}

		public static IPAddress GetRandomIpAddress()
		{
			var buffer = new byte[4];
			new Random().NextBytes(buffer);
			var result = string.Concat(buffer.Select(x => string.Format("{0}.", (x | 1).ToString())).ToArray());
			return IPAddress.Parse(result.TrimEnd('.'));
		}
	}
}