using ReactiveUI;
using System.Reflection;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
	public class AboutViewModel : ViewModelBase, IRoutableViewModel
	{
		public string? UrlPathSegment { get; } = "About";
		public IScreen? HostScreen { get; }

		#region Constructors

		public AboutViewModel() { }
		public AboutViewModel(IScreen screen)
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