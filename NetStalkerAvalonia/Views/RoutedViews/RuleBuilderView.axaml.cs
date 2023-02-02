using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Views.RoutedViews
{
	public partial class RuleBuilderView : ReactiveUserControl<RuleBuilderViewModel>
	{
		public RuleBuilderView()
		{
			this.WhenActivated(d =>
			{
				ViewModel!.ShowAddRuleDialog.RegisterHandler(DoShowAddDialogAsync).DisposeWith(d);
				ViewModel!.ShowUpdateRuleDialog.RegisterHandler(DoShowUpdateDialogAsync).DisposeWith(d);

				// Temporary fix
				// TODO: Find a better way to disable right click selection
				var listBox = this.FindControl<ListBox>("RuleList");

				listBox.ContextRequested += (sender, args) =>
				{
					listBox.SelectedItem = null;
					args.Handled = true;
				};
			});

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private async Task DoShowAddDialogAsync(InteractionContext<Unit, AddUpdateRuleModel?> interaction)
		{
			var dialog = new AddUpdateRuleWindow
			{
				DataContext = new AddUpdateRuleViewModel(isUpdate: false)
			};

			var result = await dialog.ShowDialog<AddUpdateRuleModel?>(StaticData.MainWindow);
			interaction.SetOutput(result);
		}

		private async Task DoShowUpdateDialogAsync(InteractionContext<AddUpdateRuleModel, AddUpdateRuleModel> interaction)
		{
			var dialog = new AddUpdateRuleWindow
			{
				DataContext = new AddUpdateRuleViewModel(isUpdate: true)
				{
					AddUpdateRuleModel = interaction.Input
				}
			};

			var result = await dialog.ShowDialog<AddUpdateRuleModel>(StaticData.MainWindow);
			interaction.SetOutput(result);
		}
	}
}
