using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views;

public partial class AdapterSelectWindow : ReactiveWindow<AdapterSelectViewModel>
{
    public AdapterSelectWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}