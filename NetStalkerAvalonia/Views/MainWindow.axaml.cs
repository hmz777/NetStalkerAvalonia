using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.Mixins;
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
                ViewModel!
                    .ShowLimitDialogInteraction!
                    .RegisterHandler(DoShowLimitDialogAsync)
                    .DisposeWith(disposables);

                ViewModel!
                    .ShowStatusMessageInteraction!
                    .RegisterHandler(DoShowMessageDialogAsync)
                    .DisposeWith(disposables);

                var adapterSelect = new AdapterSelectWindow();
                adapterSelect.DataContext = new AdapterSelectViewModel();

                adapterSelect.ShowDialog(this).DisposeWith(disposables);
            });

            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowLimitDialogAsync(
            InteractionContext<DeviceLimitsModel?, DeviceLimitsModel?> interaction)
        {
            var dialog = new LimitDialogWindow();
            dialog.DataContext = new LimitDialogViewModel() { DeviceLimits = interaction.Input };

            var result = await dialog.ShowDialog<DeviceLimitsModel>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowMessageDialogAsync(InteractionContext<StatusMessage, Unit> interaction)
        {
            var statusMessageDialog = new StatusMessageWindow();
            statusMessageDialog.DataContext = new StatusMessageViewModel() { StatusMessage = interaction.Input };

            var result = await statusMessageDialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }
    }
}