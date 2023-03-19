using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace NetStalkerAvalonia.Core.Views;

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
					DataContext = this.ViewModel._passwordViewModel
				};

				passwordWindow.ShowDialog(this).DisposeWith(disposables);
            }

        });

        AvaloniaXamlLoader.Load(this);
    }
}