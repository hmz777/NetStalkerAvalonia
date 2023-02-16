using NetStalker.Tests.AutoData.Specimens;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.PcapDeviceManagement;
using NetStalkerAvalonia.Services.Implementations.RulesService;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class RuleServiceCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IRuleService), typeof(RuleService)));
		}
	}
}
