using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetStalkerAvalonia.Components.DeviceList
{
    public partial class DeviceListView : UserControl
    {
        public DeviceListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
