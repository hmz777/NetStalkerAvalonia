using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace NetStalkerAvalonia.Views;

public partial class AdapterSelectWindow : ReactiveWindow<AdapterSelectViewModel>
{
    public AdapterSelectWindow()
    {
        this.WhenActivated(disposables =>
        {
            if (ViewModel!.IsAppLocked)
            {
                var passwordWindow = new PasswordWindow();
                passwordWindow.DataContext = new PasswordViewModel(null!);

                passwordWindow.ShowDialog(this).DisposeWith(disposables);
            }

        });
        AvaloniaXamlLoader.Load(this);
    }
}