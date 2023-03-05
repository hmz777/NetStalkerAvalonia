namespace NetStalker.Tests.AutoData.Customizations
{
	public class ServicesCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customize(new AutoMapperCustomization());
			fixture.Customize(new FileSystemCustomization());
			fixture.Customize(new RuleServiceCustomization());
			fixture.Customize(new PcapManagerCustomization());
			fixture.Customize(new DeviceNameResolverCustomization());
			fixture.Customize(new DeviceTypeIdentifierCustomization());
			fixture.Customize(new DeviceScannerCustomization());
			fixture.Customize(new BlockerRedirectorCustomization());
		}
	}
}