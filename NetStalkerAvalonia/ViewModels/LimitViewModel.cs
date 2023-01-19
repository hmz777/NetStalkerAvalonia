using System.Reactive;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class LimitViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, DeviceLimitsModel?> Apply { get; set; }
    public DeviceLimitsModel? DeviceLimits { get; set; }

    public LimitViewModel()
    {
        Apply = ReactiveCommand.Create(() => { return DeviceLimits; });
        DeviceLimits = new DeviceLimitsModel(0, 0);
    }
}