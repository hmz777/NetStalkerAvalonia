using SharpPcap;
using SharpPcap.LibPcap;
using System;

namespace NetStalkerAvalonia.Services
{
	public interface IPcapDeviceManager
	{
		/// <summary>
		/// The created device is automatically opened
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="packetArrivalHandler"></param>
		/// <returns></returns>
		public IPcapLiveDevice CreateDevice(string filter, PacketArrivalEventHandler? packetArrivalHandler, int readTimeout);

	}
}