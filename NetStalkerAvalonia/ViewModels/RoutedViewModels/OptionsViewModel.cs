using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class OptionsViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "Options";
        public IScreen HostScreen { get; }

        public OptionsViewModel(IScreen screen) => this.HostScreen = screen;
    }
}