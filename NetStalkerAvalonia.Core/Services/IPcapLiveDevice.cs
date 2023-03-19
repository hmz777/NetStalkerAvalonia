using PacketDotNet;
using SharpPcap;
using System;

namespace NetStalkerAvalonia.Core.Services
{
	public interface IPcapLiveDevice : IDisposable
	{
		void SendPacket(Packet packet);
		ARP CreateArp();
		void StartCapture();

		public bool Opened { get; }
		public bool Started { get; }

		public void Dispose(PacketArrivalEventHandler eventHandler);
	}
}
