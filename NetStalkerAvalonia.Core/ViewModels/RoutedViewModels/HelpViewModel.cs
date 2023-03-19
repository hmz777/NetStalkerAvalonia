using NetStalkerAvalonia.Core.Services;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.ViewModels.RoutedViewModels
{
    public class HelpViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "Help";
        public IScreen? HostScreen { get; }

        #region Constructors

#if DEBUG

        public HelpViewModel()
        {

        }

#endif

		[Splat.DependencyInjectionConstructor]
		public HelpViewModel(IRouter screen) => this.HostScreen = screen;

        #endregion
    }
}