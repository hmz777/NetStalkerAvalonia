using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.DeviceNameResolving;
using System.IO.Abstractions;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class DeviceNameResolverCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IDeviceNameResolver), typeof(DeviceNameResolver)));
			fixture.Register(() => new DeviceNameResolver(fixture.Create<IFileSystem>()));
		}
	}
}
