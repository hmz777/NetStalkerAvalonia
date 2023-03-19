using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.Views.RoutedViews
{
    public partial class HomeView : ReactiveUserControl<MainViewModel>
    {
        public HomeView()
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
