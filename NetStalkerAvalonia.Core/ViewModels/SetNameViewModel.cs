using ReactiveUI;
using System.Reactive;

namespace NetStalkerAvalonia.Core.ViewModels;

public class SetNameViewModel : ViewModelBase
{
	public string? Name { get; set; }

	public ReactiveCommand<Unit, string> Accept { get; set; }

	public SetNameViewModel()
	{
		Accept = ReactiveCommand.Create(() => Name!);
	}
}