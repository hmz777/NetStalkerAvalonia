using AutoMapper;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.RulesService;

namespace NetStalker.Tests.AutoData.Specimens
{
	public class RulesServiceSpeciment : ISpecimenBuilder
	{
		public object Create(object request, ISpecimenContext context)
		{
			if (request is IRuleService)
			{
				return new RuleService(context.Create<IMapper>());
			}

			return new NoSpecimen();
		}
	}
}