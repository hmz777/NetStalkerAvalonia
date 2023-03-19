using System.Reactive;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.Models;

public class DeviceContextMenuItem : ReactiveObject
{
    private string? _header;

    public string? Header
    {
        get => _header;
        set => this.RaiseAndSetIfChanged(ref _header, value);
    }
    public ReactiveCommand<Unit, Unit>? Command { get; set; }
}