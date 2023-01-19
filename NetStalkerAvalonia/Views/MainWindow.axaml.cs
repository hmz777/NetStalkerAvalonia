using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
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

				ViewModel!
					.SetFriendlyNameInteraction!
					.RegisterHandler(DoShowSetFriendlyDeviceNameDialogAsync)
					.DisposeWith(disposables);

				var adapterSelect = new AdapterSelectWindow
				{
					DataContext = new AdapterSelectViewModel(null!)
				};

				adapterSelect.ShowDialog(this).DisposeWith(disposables);
			});

			AvaloniaXamlLoader.Load(this);
		}

		private async Task DoShowLimitDialogAsync(
			InteractionContext<DeviceLimitsModel?, DeviceLimitsModel?> interaction)
		{
			var dialog = new LimitWindow
			{
				DataContext = new LimitViewModel() { DeviceLimits = interaction.Input }
			};

			var result = await dialog.ShowDialog<DeviceLimitsModel>(this);
			interaction.SetOutput(result);
		}

		private async Task DoShowMessageDialogAsync(InteractionContext<StatusMessageModel, Unit> interaction)
		{
			var statusMessageDialog = new StatusMessageWindow();
			statusMessageDialog.DataContext = new StatusMessageViewModel() { StatusMessage = interaction.Input };

			var result = await statusMessageDialog.ShowDialog<Unit>(this);
			interaction.SetOutput(result);
		}

		private async Task DoShowSetFriendlyDeviceNameDialogAsync(InteractionContext<string?, string?> interaction)
		{
			var setNameDialogWindow = new SetNameWindow
			{
				DataContext = new SetNameViewModel() { Name = interaction.Input }
			};

			var result = await setNameDialogWindow.ShowDialog<string?>(this);
			interaction.SetOutput(result);
		}
	}
}