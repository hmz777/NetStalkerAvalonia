using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.BlockingRedirection;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class BlockerRedirectorCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IBlockerRedirector), typeof(BlockerRedirector)));
			fixture.Register(() => new BlockerRedirector(fixture.Create<IRuleService>(), fixture.Create<IPcapDeviceManager>()));
		}
	}
}