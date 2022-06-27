using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            this.WhenActivated(disposables =>
            {
                disposables(ViewModel!.ShowLimitDialog!.RegisterHandler(DoShowDialogAsync));

                var adapterSelect = new AdapterSelectWindow();
                adapterSelect.DataContext = new AdapterSelectViewModel();

                disposables(adapterSelect.ShowDialog(this));
            });

            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowDialogAsync(InteractionContext<Unit, DeviceLimitResult?> interaction)
        {
            var dialog = new LimitDialogWindow();
            dialog.DataContext = new LimitDialogViewModel();

            var result = await dialog.ShowDialog<DeviceLimitResult>(this);
            interaction.SetOutput(result);
        }
    }
}