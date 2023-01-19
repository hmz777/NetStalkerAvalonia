using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace NetStalkerAvalonia.ViewModels
{
	public class AddUpdateRuleViewModel : ViewModelBase
	{
		private string? target;
		public string? Target
		{
			get => target;
			set => this.RaiseAndSetIfChanged(ref target, value);
		}

		private RuleAction action;
		public RuleAction Action
		{
			get => action;
			set => this.RaiseAndSetIfChanged(ref action, value);
		}

		private RuleSourceValue ruleSourceValue;
		public RuleSourceValue RuleSourceValue
		{
			get => ruleSourceValue;
			set => this.RaiseAndSetIfChanged(ref ruleSourceValue, value);
		}

		private int upload;
		public int Upload
		{
			get => upload;
			set => this.RaiseAndSetIfChanged(ref upload, value);
		}

		private int download;
		public int Download
		{
			get => download;
			set => this.RaiseAndSetIfChanged(ref download, value);
		}

		private bool isRegex;
		public bool IsRegex
		{
			get => isRegex;
			set => this.RaiseAndSetIfChanged(ref isRegex, value);
		}

		private bool active;
		public bool Active
		{
			get => active;
			set => this.RaiseAndSetIfChanged(ref active, value);
		}

		private readonly ObservableAsPropertyHelper<bool> isLimit;
		public bool IsLimit => isLimit.Value;

		public ReactiveCommand<Unit, AddUpdateRuleModel?> Accept { get; set; }

		public AddUpdateRuleModel? AddUpdateRuleModel { get; set; }

		public AddUpdateRuleViewModel()
		{
			Accept = ReactiveCommand.Create(AcceptImpl);

			isLimit = this.WhenAnyValue(x => x.Action)
					  .Select(x => x == RuleAction.Limit)
					  .ToProperty(this, x => x.IsLimit);
		}

		public AddUpdateRuleModel? AcceptImpl()
		{
			return AddUpdateRuleModel;
		}
	}
}