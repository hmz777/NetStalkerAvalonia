using System;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views;

public partial class LimitDialogWindow : ReactiveWindow<LimitDialogViewModel>
{
    public LimitDialogWindow()
    {
        this.WhenActivated(disposables => { disposables(ViewModel!.Apply.Subscribe(Close)); });

        AvaloniaXamlLoader.Load(this);
    }
}