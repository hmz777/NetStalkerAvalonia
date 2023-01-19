using NetStalkerAvalonia.Models;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NetStalkerAvalonia.Rules.Implementations
{
	public class BlockRule : RuleBase
	{
		public override RuleAction Action => RuleAction.Block;

		public BlockRule(RuleSourceValue sourceValue, bool isRegex, string target, int order, bool active)
			: base(sourceValue, isRegex, target, order, active)
		{
		}

		public override void Apply(Device device)
		{
			if (Match(device) == false)
				return;

			device.Block();
		}

		public override string ToString()
		{
			return $"Order:{Order} - Apply On:{SourceValue} - Type:{Action} - Target:{Target} - Regex:{IsRegex} - Status:{(Active ? "Enabled" : "Disabled")}";
		}
	}
}