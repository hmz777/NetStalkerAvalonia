using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.DeviceScanning;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class DeviceScannerCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IDeviceScanner), typeof(DeviceScanner)));
			fixture.Register(() => new DeviceScanner(fixture.Create<IPcapDeviceManager>(), fixture.Create<IDeviceNameResolver>(), fixture.Create<IDeviceTypeIdentifier>()));
		}
	}
}