using NetStalkerAvalonia.Services.Implementations.RulesService;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.PcapDeviceManagement;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class PcapManagerCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IPcapDeviceManager), typeof(PcapDeviceManager)));
		}
	}
}
