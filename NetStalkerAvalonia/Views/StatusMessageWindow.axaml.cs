using System;
using System.Windows.Forms.VisualStyles;
using System;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views;

public partial class StatusMessageWindow : ReactiveWindow<StatusMessageViewModel>
{
    public StatusMessageWindow()
    {
        this.WhenActivated(disposables => { ViewModel!.Close.Subscribe(x => Close()).DisposeWith(disposables); });

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}