using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class AboutViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "About";
        public IScreen HostScreen { get; }
        public AboutViewModel(IScreen screen) => this.HostScreen = screen;
    }
}