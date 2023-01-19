using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Rules.Implementations;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace NetStalkerAvalonia.Services.Implementations.RulesService
{
	public class RuleService : ReactiveObject, IRuleService
	{
		private readonly List<IRule> rules = new()
		{
			new BlockRule(RuleSourceValue.IpAddress,false,"192.168.1.144",1,true),
			new RedirectRule(RuleSourceValue.MacAddress,false,"192.168.1.144",1,true),
			new BlockRule(RuleSourceValue.IpAddress,true,"192.168.1.144",1,true),
			new BlockRule(RuleSourceValue.IpAddress,false,"192.168.1.144",1,true),
			new RedirectRule(RuleSourceValue.IpAddress,false,"192.168.1.144",1,false),
			new BlockRule(RuleSourceValue.IpAddress,false,"192.168.1.144",1,true),
			new BlockRule(RuleSourceValue.IpAddress,false,"192.168.1.144",1,true),
		};

		public IEnumerable<IRule> Rules => rules.AsReadOnly();

		public bool Status => throw new NotImplementedException();

		public void AddBlockingRule(BlockRule blockRule)
		{
			ArgumentNullException.ThrowIfNull(blockRule, nameof(blockRule));
			rules.Add(blockRule);
		}

		public void AddLimitingRule(LimitRule limitRule)
		{
			ArgumentNullException.ThrowIfNull(limitRule, nameof(limitRule));
			rules.Add(limitRule);
		}

		public void AddRedirectingRule(RedirectRule redirectRule)
		{
			ArgumentNullException.ThrowIfNull(redirectRule, nameof(redirectRule));
			rules.Add(redirectRule);
		}

		public void ApplyIfMatch(Device device)
		{
			var lastRuleThatMatch = rules.Where(r => r.Match(device)).OrderBy(r => r.Order).LastOrDefault();
			lastRuleThatMatch?.Apply(device);
		}
	}
}