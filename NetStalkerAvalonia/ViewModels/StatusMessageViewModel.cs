using System.Reactive;
using NetStalkerAvalonia.Models;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class StatusMessageViewModel : ViewModelBase
{
    public StatusMessage? StatusMessage { get; set; }
    public ReactiveCommand<Unit, Unit> Close { get; set; }

    public StatusMessageViewModel()
    {
        Close = ReactiveCommand.Create(() => Unit.Default);
    }
}