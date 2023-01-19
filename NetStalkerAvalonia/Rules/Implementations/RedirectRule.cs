﻿using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Rules.Implementations
{
	public class RedirectRule : RuleBase
	{
		public override RuleAction Action => RuleAction.Redirect;

		public RedirectRule(RuleSourceValue sourceValue, bool isRegex, string target, int order, bool active)
			: base(sourceValue, isRegex, target, order, active)
		{
		}

		public override void Apply(Device device)
		{
			if (Match(device) == false)
				return;

			device.Redirect();
		}

		public override string ToString()
		{
			return $"Order:{Order} - Apply On:{SourceValue} - Type:{Action} - Target:{Target} - Regex:{IsRegex} - Status:{(Active ? "Enabled" : "Disabled")}";
		}
	}
}