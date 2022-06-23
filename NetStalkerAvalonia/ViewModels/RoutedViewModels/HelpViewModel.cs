using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class HelpViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "Help";
        public IScreen? HostScreen { get; }
        
        #region Constructors

        public HelpViewModel()
        {
        }

        public HelpViewModel(IScreen screen) => this.HostScreen = screen;

        #endregion
    }
}