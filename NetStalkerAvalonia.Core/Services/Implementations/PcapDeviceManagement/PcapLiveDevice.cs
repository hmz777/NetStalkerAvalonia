using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetStalkerAvalonia.Services.Implementations.PcapDeviceManagement
{
	public class PcapLiveDevice : IPcapLiveDevice
	{
		private readonly LibPcapLiveDevice libPcapLiveDevice;

		public PcapLiveDevice(LibPcapLiveDevice libPcapLiveDevice)
		{
			this.libPcapLiveDevice = libPcapLiveDevice;
		}

		public bool Opened => libPcapLiveDevice.Opened;

		public bool Started => libPcapLiveDevice.Started;

		public ARP CreateArp()
		{
			return new ARP(libPcapLiveDevice);
		}

		public void Dispose(PacketArrivalEventHandler eventHandler)
		{
			libPcapLiveDevice.OnPacketArrival -= eventHandler;
			Dispose();
		}

		public void Dispose()
		{
			libPcapLiveDevice.StopCapture();
			libPcapLiveDevice.Close();
			libPcapLiveDevice.Dispose();
		}

		public void SendPacket(Packet packet)
		{
			libPcapLiveDevice.SendPacket(packet);
		}

		public void StartCapture()
		{
			libPcapLiveDevice.StartCapture();
		}
	}
}