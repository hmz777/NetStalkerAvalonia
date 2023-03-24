using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations.ErrorHandling;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class ErrorHandlerCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IErrorHandler), typeof(ErrorHandler)));
			fixture.Register(() => new ErrorHandler(fixture.Create<IStatusMessageService>()));
		}
	}
}
