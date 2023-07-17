using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations;
using NetStalkerAvalonia.Core.Services.Implementations.StatusMessages;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class StatusMessageCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IStatusMessageService), typeof(StatusMessageService)));
			fixture.Register(() => new StatusMessageService());
		}
	}
}