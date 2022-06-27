using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetStalkerAvalonia.Components;

public partial class LoadingSpinner : UserControl
{
    public LoadingSpinner()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}