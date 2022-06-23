using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class AboutViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "About";
        public IScreen? HostScreen { get; }

        #region Constructors

        public AboutViewModel(){}
        public AboutViewModel(IScreen screen) => HostScreen = screen;

        #endregion
    }
}