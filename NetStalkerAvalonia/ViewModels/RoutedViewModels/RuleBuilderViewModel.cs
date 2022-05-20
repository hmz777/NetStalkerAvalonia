using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
    public class RuleBuilderViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = "Rule Builder";
        public IScreen HostScreen { get; }
        public RuleBuilderViewModel(IScreen screen) => this.HostScreen = screen;
    }
}