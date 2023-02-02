using NetStalkerAvalonia.Models;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

namespace NetStalkerAvalonia.Rules.Implementations
{
	public class BlockRule : RuleBase
	{
		[JsonIgnore]
		public override RuleAction Action => RuleAction.Block;

		[JsonConstructor]
		public BlockRule(RuleSourceValue sourceValue, bool isRegex, string target, int order, bool active, Guid ruleId = default)
			: base(sourceValue, isRegex, target, order, active, ruleId)
		{
		}

		public override void Apply(Device device)
		{
			if (Match(device) == false)
				return;

			device.ResetState();
			device.Block();
		}

		public override string ToString()
		{
			return $"Order:{Order} - Apply On:{SourceValue} - Type:{Action} - Target:{Target} - Regex:{IsRegex} - Status:{(Active ? "Enabled" : "Disabled")}";
		}
	}
}