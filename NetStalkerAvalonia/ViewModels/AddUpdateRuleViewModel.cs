using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;

namespace NetStalkerAvalonia.ViewModels
{
	public class AddUpdateRuleViewModel : ViewModelBase
	{
		private IEnumerable<string>? ruleActions;
		public IEnumerable<string>? RuleActions
		{
			get => ruleActions;
			set => this.RaiseAndSetIfChanged(ref ruleActions, value);
		}

		private IEnumerable<string>? ruleSourceValues;
		public IEnumerable<string>? RuleSourceValues
		{
			get => ruleSourceValues;
			set => this.RaiseAndSetIfChanged(ref ruleSourceValues, value);
		}

		private readonly ObservableAsPropertyHelper<bool> isLimitRule;
		public bool IsLimitRule => isLimitRule.Value;

		private AddUpdateRuleModel? _addUpdateRuleModel;
		public AddUpdateRuleModel? AddUpdateRuleModel
		{
			get => _addUpdateRuleModel;
			set => this.RaiseAndSetIfChanged(ref _addUpdateRuleModel, value);
		}

		private bool isUpdate;
		public bool IsUpdate
		{
			get => isUpdate;
			set => this.RaiseAndSetIfChanged(ref isUpdate, value);
		}

		public ReactiveCommand<Unit, AddUpdateRuleModel?> Accept { get; set; }

		public AddUpdateRuleViewModel(bool isUpdate = false)
		{
			AddUpdateRuleModel = new AddUpdateRuleModel();

			var canAcceptRule = this.WhenAnyValue(
				x => x.AddUpdateRuleModel!.Action,
				x => x.AddUpdateRuleModel!.SourceValue,
				x => x.AddUpdateRuleModel!.Target,
				x => x.AddUpdateRuleModel!.Upload,
				x => x.AddUpdateRuleModel!.Download,
				(action, source, target, upload, download) =>
				action != null && source != null && !string.IsNullOrWhiteSpace(target) && upload >= 0 && download >= 0);

			Accept = ReactiveCommand.Create(AcceptImpl, canAcceptRule);

			isLimitRule = this.WhenAnyValue(x => x.AddUpdateRuleModel!.Action)
					  .Select(x => x == RuleAction.Limit)
					  .ToProperty(this, x => x.IsLimitRule);

			RuleActions = Enum.GetNames(typeof(RuleAction));
			RuleSourceValues = Enum.GetNames(typeof(RuleSourceValue));
			IsUpdate = isUpdate;
		}

		public AddUpdateRuleModel? AcceptImpl()
		{
			return AddUpdateRuleModel;
		}
	}
}