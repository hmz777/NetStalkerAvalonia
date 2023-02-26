using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace NetStalkerAvalonia.Views;

public partial class AdapterSelectView : ReactiveWindow<AdapterSelectViewModel>
{
    public AdapterSelectView()
    {
        this.WhenActivated(disposables =>
        {
            if (ViewModel!.IsAppLocked)
            {
				var passwordWindow = new PasswordView
				{
					DataContext = new PasswordViewModel(null!)
				};

				passwordWindow.ShowDialog(this).DisposeWith(disposables);
            }

        });

        AvaloniaXamlLoader.Load(this);
    }
}