using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Windows.Services.Implementations;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class AppLockCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IAppLockService), typeof(AppLockManagerWindows)));
			fixture.Register(() => new AppLockManagerWindows());
		}
	}
}