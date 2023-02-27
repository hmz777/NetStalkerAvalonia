using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using NetStalker.Tests.AutoData.Customizations;

namespace NetStalker.Tests.AutoData
{
	public class AutoServiceDataAttribute : AutoDataAttribute
	{
		public AutoServiceDataAttribute() : base(() =>
		{
			var fixture = new Fixture();
			fixture.Customize(new DeviceCustomization());
			fixture.Customize(new ServicesCustomization());

			return fixture;
		})
		{ }
	}
}