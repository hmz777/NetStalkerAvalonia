using System.Reactive;
using System.Reactive.Linq;
using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.ViewModels;

public class LimitViewModel : ViewModelBase
{
	public ReactiveCommand<Unit, DeviceLimitsModel> Apply { get; set; }
	public DeviceLimitsModel DeviceLimits { get; set; }

	public LimitViewModel()
	{
		DeviceLimits = new DeviceLimitsModel(0, 0);

		var capApplyExecute = this.WhenAnyValue(x => x.DeviceLimits)
			.Select(x => x.Upload >= 0 && x.Download >= 0);

		Apply = ReactiveCommand.Create(() => { return DeviceLimits; }, capApplyExecute);
	}
}