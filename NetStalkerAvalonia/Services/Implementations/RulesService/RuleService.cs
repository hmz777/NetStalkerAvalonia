using AutoMapper;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Rules.Implementations;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NetStalkerAvalonia.Services.Implementations.RulesService
{
	public class RuleService : ReactiveObject, IRuleService
	{
		private const string RulesFileName = "Rules.json";

		private readonly IMapper mapper;

		private readonly ObservableCollection<RuleBase> rules;

		public RuleService(IMapper mapper = null!)
		{
			this.mapper = Tools.ResolveIfNull(mapper);

			rules = new ObservableCollection<RuleBase>(LoadRules());

			Rules = new ReadOnlyObservableCollection<RuleBase>(rules);
		}

		public ReadOnlyObservableCollection<RuleBase> Rules { get; private set; }

		public bool Status => throw new NotImplementedException();

		private IEnumerable<RuleBase> LoadRules()
		{
			if (File.Exists(RulesFileName) == false)
				return Enumerable.Empty<RuleBase>();

			var json = File.ReadAllText(RulesFileName);

			if (json.Length == 0)
			{
				return Enumerable.Empty<RuleBase>();
			}

			try
			{
				return JsonSerializer.Deserialize<IEnumerable<RuleBase>>(json, Config.JsonSerializerOptions) ?? Enumerable.Empty<RuleBase>();
			}
			catch
			{
				Tools.ShowMessage(new StatusMessageModel(MessageType.Error, "Rules file is corrupted. It will be skipped."));
			}

			return Enumerable.Empty<RuleBase>();
		}

		public void SaveRules()
		{
			File.WriteAllText(RulesFileName, JsonSerializer.Serialize(rules, typeof(ObservableCollection<RuleBase>), Config.JsonSerializerOptions));
		}

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