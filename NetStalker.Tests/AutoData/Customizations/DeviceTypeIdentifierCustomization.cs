using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.DeviceTypeIdentification;
using System.IO.Abstractions;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class DeviceTypeIdentifierCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IDeviceTypeIdentifier), typeof(DeviceTypeIdentifier)));
			fixture.Register(() => new DeviceTypeIdentifier(fixture.Create<IFileSystem>()));
		}
	}
}