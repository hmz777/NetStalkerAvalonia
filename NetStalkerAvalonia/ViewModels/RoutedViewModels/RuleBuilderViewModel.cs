using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
	public class RuleBuilderViewModel : ViewModelBase, IRoutableViewModel
	{
		#region Services

		private readonly IRuleService ruleService;

		#endregion

		#region Routing

		public string? UrlPathSegment { get; } = "Rule Builder";
		public IScreen HostScreen { get; }

		#endregion

		#region Constructors

		public RuleBuilderViewModel() { }
		public RuleBuilderViewModel(IScreen screen, IRuleService ruleService = null!)
		{
			this.HostScreen = screen;
			this.ruleService = Tools.ResolveIfNull<IRuleService>(ruleService);
		}

		#endregion

		//private readonly ObservableAsPropertyHelper<List<IRule>> _rules;
		public IEnumerable<IRule>? Rules => ruleService?.Rules;
	}
}