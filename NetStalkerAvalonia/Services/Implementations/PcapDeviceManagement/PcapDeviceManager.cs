using SharpPcap;
using SharpPcap.LibPcap;
using System.Linq;

namespace NetStalkerAvalonia.Services.Implementations.PcapDeviceManagement
{
	public class PcapDeviceManager : IPcapDeviceManager
	{
		private string GetAdapterName()
		{
			return (from devicex in LibPcapLiveDeviceList.Instance
					where devicex.Interface.FriendlyName == HostInfo.NetworkAdapterName
					select devicex).ToList()[0].Name;
		}

		public IPcapLiveDevice CreateDevice(string filter, PacketArrivalEventHandler? packetArrivalHandler, int readTimeout)
		{
			var device = LibPcapLiveDeviceList.New()[GetAdapterName()];
			device.Open(DeviceModes.Promiscuous, readTimeout);
			device!.Filter = filter;
			device.OnPacketArrival += packetArrivalHandler;

			return new PcapLiveDevice(device);
		}
	}
}