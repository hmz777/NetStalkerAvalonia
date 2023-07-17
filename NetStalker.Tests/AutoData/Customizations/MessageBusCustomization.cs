using NetStalkerAvalonia.Core.Services.Implementations;
using NetStalkerAvalonia.Core.Services.Implementations.MessageBus;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class MessageBusCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IMessageBusService), typeof(MessageBusService)));
			fixture.Register(() => new MessageBusService());
		}
	}
}
