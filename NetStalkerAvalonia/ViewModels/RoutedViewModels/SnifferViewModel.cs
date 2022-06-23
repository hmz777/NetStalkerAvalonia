using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class SnifferViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "Packet Sniffer";
        public IScreen? HostScreen { get; }

        #region Constructors

        public SnifferViewModel(){}
        public SnifferViewModel(IScreen screen) => this.HostScreen = screen;

        #endregion
    }
}