using NetStalkerAvalonia.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Rules.Implementations
{
	public class RedirectRule : RuleBase
	{
		[JsonIgnore]
		public override RuleAction Action => RuleAction.Redirect;

		[JsonConstructor]
		public RedirectRule(RuleSourceValue sourceValue, bool isRegex, string target, int order, bool active, Guid ruleId = default)
			: base(sourceValue, isRegex, target, order, active, ruleId)
		{
		}

		public override void Apply(Device device)
		{
			if (Match(device) == false)
				return;

			device.ResetState();
			device.Redirect();
		}

		public override void UnApply(Device device)
		{
			if (Match(device) == false)
				return;

			device.ResetState();
		}

		public override string ToString()
		{
			return $"Order:{Order} - Apply On:{SourceValue} - Type:{Action} - Target:{Target} - Regex:{IsRegex} - Status:{(Active ? "Enabled" : "Disabled")}";
		}
	}
}