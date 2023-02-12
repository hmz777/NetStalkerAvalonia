using AutoFixture;
using AutoFixture.Xunit2;
using NetStalker.Tests.AutoData.Customizations;

namespace NetStalker.Tests.AutoData
{
	public class AutoDeviceDataAttribute : AutoDataAttribute
	{
		public AutoDeviceDataAttribute() : base(() =>
		{
			var fixture = new Fixture();
			fixture.Customize(new DeviceCustomization());

			return fixture;
		})
		{ }
	}
}