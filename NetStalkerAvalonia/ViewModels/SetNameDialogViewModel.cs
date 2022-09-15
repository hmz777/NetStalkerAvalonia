using System.Reactive;
using System.Windows.Documents.DocumentStructures;
using Windows.System.Profile;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels;

public class SetNameDialogViewModel : ViewModelBase
{
    public string? Name { get; set; }

    public ReactiveCommand<Unit, string> Accept { get; set; }

    public SetNameDialogViewModel()
    {
        Accept = ReactiveCommand.Create(() => Name!);
    }
}