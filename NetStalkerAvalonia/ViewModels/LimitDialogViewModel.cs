using System.Reactive;
using NetStalkerAvalonia.Models;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class LimitDialogViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, DeviceLimitResult?> Apply { get; set; }
    public DeviceLimitResult? DeviceLimitResult { get; set; }

    public LimitDialogViewModel()
    {
        Apply = ReactiveCommand.Create(() => { return DeviceLimitResult; });
        DeviceLimitResult = new DeviceLimitResult();
    }
}