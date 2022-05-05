using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetStalkerAvalonia.Views.RoutedViews
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
