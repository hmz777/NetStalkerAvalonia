using AutoMapper;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class ViewModelsCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Register(() => new SnifferViewModel(fixture.Create<IRouter>()));
			fixture.Register(() => new OptionsViewModel(fixture.Create<IRouter>(), fixture.Create<IAppLockService>()));
			fixture.Register(() => new RuleBuilderViewModel(fixture.Create<IRouter>(), fixture.Create<IRuleService>(), fixture.Create<IMapper>()));
			fixture.Register(() => new HelpViewModel(fixture.Create<IRouter>()));
			fixture.Register(() => new AboutViewModel(fixture.Create<IRouter>()));
			fixture.Register(() => new AppLogViewModel());
			fixture.Register(() => new AdapterSelectViewModel(fixture.Create<IPcapDeviceManager>(), fixture.Create<IAppLockService>()));
		}
	}
}