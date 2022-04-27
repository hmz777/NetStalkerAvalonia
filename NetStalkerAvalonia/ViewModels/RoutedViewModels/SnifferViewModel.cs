using ReactiveUI;
using System;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class SnifferViewModel : ViewModelBase, IRoutableViewModel
    {
        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public SnifferViewModel(IScreen screen) => HostScreen = screen;
    }
}
