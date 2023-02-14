using NetStalkerAvalonia.Models;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Services.Implementations.PcapDeviceManagement
{
	public class PcapDeviceManager : IPcapDeviceManager
	{
		private readonly string _adapterName;

		public PcapDeviceManager()
		{
			_adapterName = (from devicex in LibPcapLiveDeviceList.Instance
							where devicex.Interface.FriendlyName == HostInfo.NetworkAdapterName
							select devicex).ToList()[0].Name;
		}

		public LibPcapLiveDevice CreateDevice(string filter, PacketArrivalEventHandler packetArrivalHandler)
		{
			var device = LibPcapLiveDeviceList.New()[_adapterName];
			device.Open(DeviceModes.Promiscuous, 1000);
			device.Filter = filter;
			device.OnPacketArrival += packetArrivalHandler;

			return device;
		}
	}
}