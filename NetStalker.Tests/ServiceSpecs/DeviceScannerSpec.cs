using FluentAssertions;
using NetStalker.Tests.AutoData;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.DeviceScanning;

namespace NetStalker.Tests.ServiceSpecs
{
	public class DeviceScannerSpec
	{
		[Theory, AutoServiceData]
		public void Can_Start_Scanner(IPcapDeviceManager pcapDeviceManager, IDeviceNameResolver deviceNameResolver, IDeviceTypeIdentifier deviceTypeIdentifier)
		{
			var sut = new DeviceScanner(pcapDeviceManager, deviceNameResolver, deviceTypeIdentifier);

			var action = () => sut.Scan();

			action.Should().NotThrow<Exception>();
			sut.Status.Should().Be(true);
		}

		[Theory, AutoServiceData]
		public void Can_Do_A_Scan_Refresh(IPcapDeviceManager pcapDeviceManager, IDeviceNameResolver deviceNameResolver, IDeviceTypeIdentifier deviceTypeIdentifier)
		{
			var sut = new DeviceScanner(pcapDeviceManager, deviceNameResolver, deviceTypeIdentifier);
			sut.Scan();

			var action = () => sut.Refresh();

			action.Should().NotThrow<Exception>();
			sut.Status.Should().Be(true);
		}
	}
}