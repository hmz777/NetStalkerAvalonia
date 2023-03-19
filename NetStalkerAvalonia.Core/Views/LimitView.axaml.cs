using System;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.Views;

public partial class LimitView : ReactiveWindow<LimitViewModel>
{
    public LimitView()
    {
        this.WhenActivated(disposables =>
        {
            ViewModel!.Apply.Subscribe(Close).DisposeWith(disposables);
        });

        AvaloniaXamlLoader.Load(this);
    }
}