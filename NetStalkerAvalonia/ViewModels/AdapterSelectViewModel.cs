using System.Reactive;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class AdapterSelectViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> Exit { get; set; }

    public AdapterSelectViewModel()
    {
        Exit = ReactiveCommand.Create(() =>
        {
            var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            app?.Shutdown();
        });
    }
}