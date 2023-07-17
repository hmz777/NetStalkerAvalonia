using AutoMapper;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.ViewModels;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class ViewModelsCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Register(() => new SnifferViewModel(fixture.Create<IRouter>()));
			fixture.Register(() => new OptionsViewModel(fixture.Create<IRouter>(), fixture.Create<IAppLockService>()));
			fixture.Register(() => new RuleBuilderViewModel(fixture.Create<IRouter>(), fixture.Create<IRuleService>(), fixture.Create<IMapper>(), fixture.Create<IErrorHandler>()));
			fixture.Register(() => new HelpViewModel(fixture.Create<IRouter>()));
			fixture.Register(() => new AboutViewModel(fixture.Create<IRouter>()));
			fixture.Register(() => new AppLogViewModel());
			fixture.Register(() => new PasswordViewModel(fixture.Create<IAppLockService>()));
			fixture.Register(() => new AdapterSelectViewModel(fixture.Create<IPcapDeviceManager>(), fixture.Create<IAppLockService>(), fixture.Create<PasswordViewModel>()));
		}
	}
}