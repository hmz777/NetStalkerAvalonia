using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.Views.RoutedViews
{
    public partial class HelpView : ReactiveUserControl<HelpViewModel>
    {
        public HelpView()
        {
            this.WhenActivated(disposables => { });
            InitializeComponent();
        }
    }
}
