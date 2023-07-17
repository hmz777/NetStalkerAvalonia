using Moq;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.PcapDeviceManagement;
using SharpPcap;

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

				pcapDeviceManagerMock.Setup(p => p.CreateDevice(It.IsAny<string>(), It.IsAny<PacketArrivalEventHandler?>(), It.IsAny<int>(), It.IsAny<string>()))
				.Returns(pcapLiveDeviceMock.Object);

				return pcapDeviceManagerMock.Object;
			});
		}
	}
}