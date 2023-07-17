using System.Reactive;
using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.ViewModels;

public class StatusMessageViewModel : ViewModelBase
{
    public StatusMessageModel? StatusMessage { get; set; }
    public ReactiveCommand<Unit, Unit> Close { get; set; }

    public StatusMessageViewModel()
    {
        Close = ReactiveCommand.Create(() => Unit.Default);
    }
}