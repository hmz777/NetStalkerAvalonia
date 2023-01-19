using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Rules.Implementations
{
	public class LimitRule : RuleBase
	{
		public override RuleAction Action => RuleAction.Limit;
		public int Upload { get; private set; }
		public int Download { get; private set; }

		public LimitRule(RuleSourceValue sourceValue, bool isRegex, string target, int upload, int download, int order, bool active)
			: base(sourceValue, isRegex, target, order, active)
		{

			Upload = upload;
			Download = download;
		}

		public override void Apply(Device device)
		{
			if (Match(device) == false)
				return;

			device.SetUploadCap(Upload);
			device.SetDownloadCap(Download);
		}

		public override string ToString()
		{
			return $"Order:{Order} - Apply On:{SourceValue} - Type:{Action} - Target:{Target} - U:{Upload}Kb/s D:{Download}Kb/s - Regex:{IsRegex} - Status:{(Active ? "Enabled" : "Disabled")}";
		}
	}
}