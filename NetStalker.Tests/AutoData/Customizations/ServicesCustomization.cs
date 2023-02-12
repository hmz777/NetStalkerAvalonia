using AutoMapper;
using NetStalker.Tests.AutoData.Specimens;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.RulesService;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class ServicesCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IMapper), typeof(Mapper)));
			fixture.Register<IMapper>(Tools.BuildAutoMapper);
			fixture.Customizations.Add(new TypeRelay(typeof(IRuleService), typeof(RuleService)));
			fixture.Customizations.Add(new RulesServiceSpeciment());
		}
	}
}
