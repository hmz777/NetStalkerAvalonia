using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class RuleBuilderViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "Rule Builder";
        public IScreen? HostScreen { get; }

        #region Constructors
        
        public RuleBuilderViewModel(){}
        public RuleBuilderViewModel(IScreen screen) => this.HostScreen = screen;

        #endregion
    }
}