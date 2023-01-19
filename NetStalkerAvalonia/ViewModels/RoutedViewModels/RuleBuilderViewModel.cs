using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
	public class RuleBuilderViewModel : ViewModelBase, IRoutableViewModel
	{
		#region Services

		private readonly IRuleService? ruleService;

		#endregion

		#region Routing

		public string? UrlPathSegment { get; } = "Rule Builder";
		public IScreen? HostScreen { get; }

		#endregion

		#region Commands

		public ReactiveCommand<Unit, Unit> AddRule { get; set; }
		public ReactiveCommand<Unit, Unit> UpdateRule { get; set; }

		#endregion

		#region Constructors

		public RuleBuilderViewModel(IScreen screen, IRuleService ruleService = null!)
		{
			this.HostScreen = screen;
			this.ruleService = Tools.ResolveIfNull<IRuleService>(ruleService);

			AddRule = ReactiveCommand.CreateFromTask(AddRuleImpl);
			UpdateRule = ReactiveCommand.CreateFromTask(UpdateRuleImpl);
		}

		#endregion

		public IEnumerable<IRule>? Rules => ruleService?.Rules;

		private AddUpdateRuleModel selectedRule;
		public AddUpdateRuleModel SelectedRule
		{
			get => selectedRule;
			set => this.RaiseAndSetIfChanged(ref selectedRule, value);
		}

		#region Interactions

		public Interaction<Unit, AddUpdateRuleModel?> ShowAddRuleDialog { get; } = new Interaction<Unit, AddUpdateRuleModel?>();
		public Interaction<AddUpdateRuleModel, AddUpdateRuleModel> ShowUpdateRuleDialog { get; } = new Interaction<AddUpdateRuleModel, AddUpdateRuleModel>();

		#endregion

		#region Handlers

		public async Task<Unit> AddRuleImpl()
		{
			var result = await ShowAddRuleDialog.Handle(Unit.Default);

			// TODO: Add rule through service

			return Unit.Default;
		}

		public async Task<Unit> UpdateRuleImpl()
		{
			var result = await ShowUpdateRuleDialog.Handle(SelectedRule);

			// TODO: Update rule through service

			return Unit.Default;
		}

		#endregion
	}
}