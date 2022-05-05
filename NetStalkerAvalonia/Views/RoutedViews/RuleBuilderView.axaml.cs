using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetStalkerAvalonia.Views.RoutedViews
{
    public partial class RuleBuilderView : UserControl
    {
        public RuleBuilderView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
