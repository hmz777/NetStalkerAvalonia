using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.DeviceNameResolving;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
