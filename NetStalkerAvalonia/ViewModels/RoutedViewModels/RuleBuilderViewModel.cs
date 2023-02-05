using AutoMapper;
using DynamicData;
using DynamicData.Binding;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
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
		private readonly IMapper mapper;

		#endregion

		#region Routing

		public string? UrlPathSegment { get; } = "Rule Builder";
		public IScreen? HostScreen { get; }

		#endregion

		#region Commands

		public ReactiveCommand<Unit, Unit> AddRule { get; set; }
		public ReactiveCommand<Unit, Unit> UpdateRule { get; set; }
		public ReactiveCommand<Unit, Unit> RemoveRule { get; set; }
		public ReactiveCommand<Unit, Unit> MoveUp { get; set; }
		public ReactiveCommand<Unit, Unit> MoveDown { get; set; }

		#endregion

		#region Constructors

#if DEBUG

		public RuleBuilderViewModel()
		{

		}

#endif

		public RuleBuilderViewModel(IScreen screen, IRuleService ruleService = null!, IMapper mapper = null!)
		{
			#region Services

			this.HostScreen = screen;
			this.ruleService = Tools.ResolveIfNull(ruleService);
			this.mapper = Tools.ResolveIfNull(mapper);

			#endregion

			#region Commands

			var isAnItemSelected = this.WhenAnyValue(x => x.SelectedRule)
				.Select(x => x! != null!);

			AddRule = ReactiveCommand.CreateFromTask(AddRuleImpl);
			UpdateRule = ReactiveCommand.CreateFromTask(UpdateRuleImpl, isAnItemSelected);
			RemoveRule = ReactiveCommand.Create(RemoveRuleImpl, isAnItemSelected);
			MoveUp = ReactiveCommand.Create(MoveUpImpl, isAnItemSelected);
			MoveDown = ReactiveCommand.Create(MoveDownImpl, isAnItemSelected);

			#endregion

			#region Exceptions

			AddRule.ThrownExceptions.Subscribe(x =>
				Tools.HandleError(new StatusMessageModel(MessageType.Error, x.Message)));
			UpdateRule.ThrownExceptions.Subscribe(x =>
				Tools.HandleError(new StatusMessageModel(MessageType.Error, x.Message)));
			RemoveRule.ThrownExceptions.Subscribe(x =>
				Tools.HandleError(new StatusMessageModel(MessageType.Error, x.Message)));
			MoveUp.ThrownExceptions.Subscribe(x =>
				Tools.HandleError(new StatusMessageModel(MessageType.Error, x.Message)));
			MoveDown.ThrownExceptions.Subscribe(x =>
				Tools.HandleError(new StatusMessageModel(MessageType.Error, x.Message)));

			#endregion

			rules = this.ruleService.Rules
				.ToObservableChangeSet()
				.AutoRefresh()
				.ToCollection()
				.Select(x => x.OrderBy(r => r.Order).ToList())
				.ToProperty(this, x => x.Rules);
		}

		#endregion

		#region UI Properties

		private readonly ObservableAsPropertyHelper<List<RuleBase>?> rules;
		public List<RuleBase>? Rules => rules.Value ?? new List<RuleBase>();

		private RuleBase? selectedRule;
		public RuleBase? SelectedRule
		{
			get => selectedRule;
			set => this.RaiseAndSetIfChanged(ref selectedRule, value);
		}

		#endregion

		#region Interactions

		public Interaction<Unit, AddUpdateRuleModel?> ShowAddRuleDialog { get; } = new Interaction<Unit, AddUpdateRuleModel?>();
		public Interaction<AddUpdateRuleModel, AddUpdateRuleModel> ShowUpdateRuleDialog { get; } = new Interaction<AddUpdateRuleModel, AddUpdateRuleModel>();

		#endregion

		#region Handlers

		private async Task<Unit> AddRuleImpl()
		{
			var result = await ShowAddRuleDialog.Handle(Unit.Default);

			if (result == null)
				return Unit.Default;

			result.Order = Rules!.OrderBy(x => x.Order).Select(x => x.Order).LastOrDefault() + 1;

			bool opResult = true;

			switch (result.Action)
			{
				case RuleAction.Block:
					{
						opResult = ruleService!
						  .TryAddBlockingRule(new BlockRule((RuleSourceValue)result.SourceValue!, result.IsRegex, result.Target!, result.Order, result.Active));

						break;
					}
				case RuleAction.Redirect:
					{
						opResult = ruleService!
						  .TryAddRedirectingRule(new RedirectRule((RuleSourceValue)result.SourceValue!, result.IsRegex, result.Target!, result.Order, result.Active));

						break;
					}
				case RuleAction.Limit:
					{
						opResult = ruleService!
						  .TryAddLimitingRule(new LimitRule(result.Upload, result.Download, (RuleSourceValue)result.SourceValue!, result.IsRegex, result.Target!, result.Order, result.Active));

						break;
					}
				default:
					break;
			}


			if (opResult == false)
			{
				Tools.ShowMessage(new StatusMessageModel(MessageType.Error, $"Rule for target: {result.Target} already exists"));
			}

			return Unit.Default;
		}

		private async Task<Unit> UpdateRuleImpl()
		{
			var addUpdateModel = mapper.Map<AddUpdateRuleModel>(SelectedRule);
			var result = await ShowUpdateRuleDialog.Handle(addUpdateModel);

			if (result == null)
				return Unit.Default;

			var opResult = false;

			switch (SelectedRule)
			{
				case BlockRule:
					{
						opResult = ruleService!.TryUpdateRule(mapper.Map<BlockRule>(result));
						break;
					}
				case RedirectRule:
					{
						opResult = ruleService!.TryUpdateRule(mapper.Map<RedirectRule>(result));
						break;
					}
				case LimitRule:
					{
						opResult = ruleService.TryUpdateRule(mapper.Map<LimitRule>(result));
						break;
					}
				default:
					break;
			}

			if (opResult == false)
			{
				Tools.ShowMessage(new StatusMessageModel(MessageType.Error, $"Rule for target: {result.Target} doesn't exists"));
			}

			return Unit.Default;
		}

		private void RemoveRuleImpl()
		{
			var opResult = ruleService!.TryRemoveRule(SelectedRule!);

			if (opResult == false)
			{
				Tools.ShowMessage(new StatusMessageModel(MessageType.Error, $"Rule for target: {SelectedRule!.Target} doesn't exists"));
			}
		}

		private void MoveUpImpl()
		{
			if (SelectedRule is not null)
			{
				ruleService!.MoveRuleUp(selectedRule!);
			}
		}

		private void MoveDownImpl()
		{
			if (SelectedRule is not null)
			{
				ruleService!.MoveRuleDown(selectedRule!);
			}
		}

		#endregion
	}
}