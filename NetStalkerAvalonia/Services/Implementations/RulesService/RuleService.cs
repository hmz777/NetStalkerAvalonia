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
			new BlockRule(RuleSourceValue.IPAddress,false,"192.168.1.144",1,true),
			new RedirectRule(RuleSourceValue.MacAddress,false,"192.168.1.142",1,true),
			new BlockRule(RuleSourceValue.IPAddress,true,"192.168.1.141",1,true),
			new BlockRule(RuleSourceValue.IPAddress,false,"192.168.1.145",1,true),
			new RedirectRule(RuleSourceValue.IPAddress,false,"192.168.1.146",1,false),
			new BlockRule(RuleSourceValue.IPAddress,false,"192.168.1.147",1,true),
			new BlockRule(RuleSourceValue.IPAddress,false,"192.168.1.148",1,true),
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