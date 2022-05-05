using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetStalkerAvalonia.Views.RoutedViews
{
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
