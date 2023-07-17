using NetStalker.Tests.AutoData.Customizations;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.BlockingRedirection;

namespace NetStalker.Tests.ServiceSpecs
{
	public class BlockerRedirectorSpecBase : IDisposable
	{
		public IBlockerRedirector BlockerRedirectorService { get; }

		public BlockerRedirectorSpecBase()
		{
			var fixture = new Fixture();
			fixture.Customize(new ServicesCustomization());

			BlockerRedirectorService = fixture.Create<BlockerRedirector>();
		}

		public void Dispose()
		{
			BlockerRedirectorService.Dispose();
		}
	}
}
