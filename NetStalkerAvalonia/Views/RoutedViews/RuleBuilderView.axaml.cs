using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views.RoutedViews
{
    public partial class RuleBuilderView : ReactiveUserControl<RuleBuilderViewModel>
    {
        public RuleBuilderView()
        {
            this.WhenActivated(disposables => { });
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
