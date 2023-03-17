using NetStalkerAvalonia.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
	public class HomeViewModel : ViewModelBase, IRoutableViewModel
	{
		private readonly IDeviceScanner deviceScanner;
		private readonly IBlockerRedirector blockerRedirector;
		private readonly IDeviceNameResolver deviceNameResolver;

		public HomeViewModel(
			IRouter router,
			IDeviceScanner deviceScanner,
			IBlockerRedirector blockerRedirector,
			IDeviceNameResolver deviceNameResolver)
		{
			HostScreen = router;
			this.deviceScanner = deviceScanner;
			this.blockerRedirector = blockerRedirector;
			this.deviceNameResolver = deviceNameResolver;
		}

		public string? UrlPathSegment => "Device List";

		public IScreen HostScreen { get; }
	}
}
