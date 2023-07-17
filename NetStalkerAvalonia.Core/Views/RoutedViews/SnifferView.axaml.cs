using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.Views.RoutedViews
{
    public partial class SnifferView : ReactiveUserControl<SnifferViewModel>
    {
        public SnifferView()
        {
            this.WhenActivated(disposables => { });
            InitializeComponent();
        }
    }
}
