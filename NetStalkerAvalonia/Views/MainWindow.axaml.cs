using System;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            this.WhenActivated(disposables =>
            {
                var adapterSelect = new AdapterSelectWindow();
                adapterSelect.DataContext = new AdapterSelectViewModel();
                adapterSelect.ShowDialog(this);
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}