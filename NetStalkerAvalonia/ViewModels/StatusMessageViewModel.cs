using System.Reactive;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class StatusMessageViewModel : ViewModelBase
{
    public StatusMessageModel? StatusMessage { get; set; }
    public ReactiveCommand<Unit, Unit> Close { get; set; }

    public StatusMessageViewModel()
    {
        Close = ReactiveCommand.Create(() => Unit.Default);
    }
}