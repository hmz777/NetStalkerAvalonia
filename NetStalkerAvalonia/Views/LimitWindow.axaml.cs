using System;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views;

public partial class LimitWindow : ReactiveWindow<LimitViewModel>
{
    public LimitWindow()
    {
        this.WhenActivated(disposables =>
        {
            ViewModel!.Apply.Subscribe(Close).DisposeWith(disposables);
        });

        AvaloniaXamlLoader.Load(this);
    }
}