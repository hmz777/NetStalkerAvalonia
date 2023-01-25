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
				DataContext = new AddUpdateRuleViewModel()
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
