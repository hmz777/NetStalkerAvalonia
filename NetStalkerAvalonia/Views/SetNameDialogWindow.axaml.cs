using System;
using System.Reactive.Disposables;
using System.Windows.Forms.VisualStyles;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views;

public partial class SetNameDialogWindow : ReactiveWindow<SetNameDialogViewModel>
{
    public SetNameDialogWindow()
    {
        this.WhenActivated(disposables => { ViewModel!.Accept.Subscribe(Close).DisposeWith(disposables); });

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}