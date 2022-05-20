using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views.RoutedViews
{
    public partial class AboutView : ReactiveUserControl<AboutViewModel>
    {
        public AboutView()
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