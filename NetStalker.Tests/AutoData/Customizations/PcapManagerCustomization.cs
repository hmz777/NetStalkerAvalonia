using Moq;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.PcapDeviceManagement;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class PcapManagerCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IPcapDeviceManager), typeof(PcapDeviceManager)));
			fixture.Register(() =>
			{
				var pcapDeviceManagerMock = new Mock<IPcapDeviceManager>();
				var pcapLiveDeviceMock = new Mock<IPcapLiveDevice>();

				pcapDeviceManagerMock.Setup(p => p.CreateDevice(It.IsAny<string>(), It.IsAny<PacketArrivalEventHandler?>(), It.IsAny<int>())).Returns(pcapLiveDeviceMock.Object);

				return pcapDeviceManagerMock.Object;
			});
		}
	}
}