using System.Reactive;
using NetStalkerAvalonia.Models;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class LimitDialogViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, DeviceLimitsModel?> Apply { get; set; }
    public DeviceLimitsModel? DeviceLimits { get; set; }

    public LimitDialogViewModel()
    {
        Apply = ReactiveCommand.Create(() => { return DeviceLimits; });
        DeviceLimits = new DeviceLimitsModel(0, 0);
    }
}