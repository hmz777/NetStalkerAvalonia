using AutoMapper;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Rules.Implementations;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace NetStalkerAvalonia.Services.Implementations.RulesService
{
	public class RuleService : ReactiveObject, IRuleService
	{
		private readonly IMapper mapper;

		private readonly ObservableCollection<IRule> rules = new();

		public RuleService(IMapper mapper = null!)
		{
			this.mapper = Tools.ResolveIfNull(mapper);

			Rules = new ReadOnlyObservableCollection<IRule>(rules);
		}

		public ReadOnlyObservableCollection<IRule> Rules { get; private set; }

		public bool Status => throw new NotImplementedException();

		private bool RuleExists(RuleBase rule) => rules.Contains(rule);

		public bool TryAddBlockingRule(BlockRule blockRule)
		{
			ArgumentNullException.ThrowIfNull(blockRule, nameof(blockRule));

			if (RuleExists(blockRule) == true)
				return false;

			rules.Add(blockRule);

			return true;
		}

		public bool TryAddLimitingRule(LimitRule limitRule)
		{
			ArgumentNullException.ThrowIfNull(limitRule, nameof(limitRule));

			if (RuleExists(limitRule) == true)
				return false;

			rules.Add(limitRule);

			return true;
		}

		public bool TryAddRedirectingRule(RedirectRule redirectRule)
		{
			ArgumentNullException.ThrowIfNull(redirectRule, nameof(redirectRule));

			if (RuleExists(redirectRule) == true)
				return false;

			rules.Add(redirectRule);

			return true;
		}

		public void ApplyIfMatch(Device device)
		{
			var lastRuleThatMatch = rules.Where(r => r.Match(device)).OrderBy(r => r.Order).LastOrDefault();
			lastRuleThatMatch?.Apply(device);
		}

		public bool TryUpdateRule(RuleBase rule)
		{
			if (RuleExists(rule) == false)
				return false;

			var ruleToUpdate = rules.First(r => r.RuleId == rule.RuleId);
			mapper.Map(rule, ruleToUpdate);

			return true;
		}

		public bool TryRemoveRule(RuleBase rule)
		{
			if (RuleExists(rule) == false)
				return false;

			rules.Remove(rule);

			return true;
		}
	}
}