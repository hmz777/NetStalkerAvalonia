using NetStalkerAvalonia.Core.Services;
using ReactiveUI;
using System.Reflection;

namespace NetStalkerAvalonia.Core.ViewModels.RoutedViewModels
{
	public class AboutViewModel : ViewModelBase, IRoutableViewModel
	{
		public string? UrlPathSegment { get; } = "About";
		public IScreen? HostScreen { get; }

		#region Constructors

#if DEBUG

		public AboutViewModel() { }

#endif

		[Splat.DependencyInjectionConstructor]
		public AboutViewModel(IRouter screen)
		{
			HostScreen = screen;

			AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		#endregion

		private string _appVersion;
		public string AppVersion
		{
			get => _appVersion;
			set => this.RaiseAndSetIfChanged(ref _appVersion, value);
		}
	}
}