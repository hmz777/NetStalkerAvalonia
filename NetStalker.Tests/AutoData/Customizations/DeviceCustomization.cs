using NetStalker.Tests.AutoData.Specimens;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class DeviceCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new MacSpecimen());
			fixture.Customizations.Add(new IpSpecimen());
		}
	}
}