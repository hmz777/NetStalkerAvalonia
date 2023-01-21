using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Rules.Implementations;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public RuleBuilderViewModel()
		{

		}

		public RuleBuilderViewModel(IScreen screen, IRuleService ruleService = null!)
		{
			this.HostScreen = screen;
			this.ruleService = Tools.ResolveIfNull<IRuleService>(ruleService);

			AddRule = ReactiveCommand.CreateFromTask(AddRuleImpl);
			UpdateRule = ReactiveCommand.CreateFromTask(UpdateRuleImpl);
		}

		#endregion

		public IEnumerable<IRule>? Rules => ruleService?.Rules;

		private RuleBase? selectedRule;
		public RuleBase? SelectedRule
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
			switch (SelectedRule)
			{
				case BlockRule blockRule:
					{
						var result = await ShowUpdateRuleDialog.Handle(new AddUpdateRuleModel
						{
							Action = blockRule.Action,
							Active = blockRule.Active,
							IsRegex = blockRule.IsRegex,
							Order = blockRule.Order,
							SourceValue = blockRule.SourceValue,
							Target = blockRule.Target,
						});

						break;
					}
				case RedirectRule redirectRule:
					{
						var result = await ShowUpdateRuleDialog.Handle(new AddUpdateRuleModel
						{
							Action = redirectRule.Action,
							Active = redirectRule.Active,
							IsRegex = redirectRule.IsRegex,
							Order = redirectRule.Order,
							SourceValue = redirectRule.SourceValue,
							Target = redirectRule.Target,
						});

						break;
					}
				case LimitRule limitRule:
					{
						var result = await ShowUpdateRuleDialog.Handle(new AddUpdateRuleModel
						{
							Action = limitRule.Action,
							Active = limitRule.Active,
							IsRegex = limitRule.IsRegex,
							Order = limitRule.Order,
							SourceValue = limitRule.SourceValue,
							Target = limitRule.Target,
							Download = limitRule.Download,
							Upload = limitRule.Upload
						});

						break;
					}
				default:
					break;
			}

			// TODO: Update rule through service

			return Unit.Default;
		}

		#endregion
	}
}