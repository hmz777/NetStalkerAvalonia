using NetStalkerAvalonia.Theme;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public ViewModelBase()
        {
        }

        public Colors ThemeColors { get; set; } = new();

        private bool navEnabled;

        public bool NavEnabled
        {
            get => navEnabled;
            set => this.RaiseAndSetIfChanged(ref navEnabled, value);
        }
    }
}