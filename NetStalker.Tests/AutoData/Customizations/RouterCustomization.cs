using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.ViewRouting;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class RouterCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IRouter), typeof(ViewRouter)));
			fixture.Register(() => new ViewRouter());
		}
	}
}
