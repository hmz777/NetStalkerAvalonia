using AutoMapper;
using NetStalkerAvalonia.Core.Helpers;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class AutoMapperCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IMapper), typeof(Mapper)));
			fixture.Register<IMapper>(AutoMapperHelpers.BuildAutoMapper);
		}
	}
}
