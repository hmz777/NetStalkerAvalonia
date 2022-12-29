using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Helpers
{
	public static class Extensions
	{
		public static string ToOuiMac(this PhysicalAddress physicalAddress) => physicalAddress.ToString()[..6];
	}
}
