using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class AdapterSelectViewModel : ViewModelBase
{
    public ReactiveCommand<Window, Unit> Accept { get; set; }
    public ReactiveCommand<Unit, Unit> Exit { get; set; }

    public AdapterSelectViewModel()
    {
        Accept = ReactiveCommand.Create((Window window) => { window.Close(); });

        Exit = ReactiveCommand.Create(() =>
        {
            var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            app?.Shutdown();
        });
    }
}