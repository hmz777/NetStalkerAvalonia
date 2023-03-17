using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Views
{
	public partial class MainView : ReactiveWindow<MainViewModel>
	{
		public MainView()
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

				ViewModel!
					.ShowAppLogInteraction!
					.RegisterHandler(DoShowAppLogDialog)
					.DisposeWith(disposables);
			});

			AvaloniaXamlLoader.Load(this);
		}

		private void DoShowAppLogDialog(
			InteractionContext<Unit, Unit> interaction)
		{
			var dialog = new AppLogView
			{
				DataContext = ViewModel!._appLogViewModel
			};

			dialog.Show(this);
			interaction.SetOutput(Unit.Default);
		}

		private async Task DoShowLimitDialogAsync(
			InteractionContext<DeviceLimitsModel?, DeviceLimitsModel?> interaction)
		{
			var dialog = new LimitView
			{
				DataContext = new LimitViewModel() { DeviceLimits = interaction.Input }
			};

			var result = await dialog.ShowDialog<DeviceLimitsModel>(this);
			interaction.SetOutput(result);
		}

		private async Task DoShowMessageDialogAsync(InteractionContext<StatusMessageModel, Unit> interaction)
		{
			var statusMessageDialog = new StatusMessageView();
			statusMessageDialog.DataContext = new StatusMessageViewModel() { StatusMessage = interaction.Input };

			var result = await statusMessageDialog.ShowDialog<Unit>(this);
			interaction.SetOutput(result);
		}

		private async Task DoShowSetFriendlyDeviceNameDialogAsync(InteractionContext<string?, string?> interaction)
		{
			var setNameDialogWindow = new SetNameView
			{
				DataContext = new SetNameViewModel() { Name = interaction.Input }
			};

			var result = await setNameDialogWindow.ShowDialog<string?>(this);
			interaction.SetOutput(result);
		}
	}
}