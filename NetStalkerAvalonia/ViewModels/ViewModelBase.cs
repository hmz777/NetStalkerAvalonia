using NetStalkerAvalonia.Theme;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public AppTheme Theme { get; set; } = new();

        // private bool _navEnabled;
        //
        // public bool NavEnabled
        // {
        //     get => _navEnabled;
        //     set => this.RaiseAndSetIfChanged(ref _navEnabled, value);
        // }
    }
}