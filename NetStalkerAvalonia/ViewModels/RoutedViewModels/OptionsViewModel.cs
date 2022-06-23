using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class OptionsViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "Options";
        public IScreen? HostScreen { get; }

        #region Constructors

        public OptionsViewModel(){}
        public OptionsViewModel(IScreen screen) => HostScreen = screen;

        #endregion
    }
}