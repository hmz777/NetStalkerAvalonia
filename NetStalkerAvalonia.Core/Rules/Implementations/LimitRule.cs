using NetStalkerAvalonia.Core.Models;
using ReactiveUI;
using System;
using System.Text.Json.Serialization;

namespace NetStalkerAvalonia.Core.Rules.Implementations
{
	public class LimitRule : RuleBase
	{
		[JsonIgnore]
		public override RuleAction Action => RuleAction.Limit;

		private int upload;
		public int Upload
		{
			get => upload;
			protected set => this.RaiseAndSetIfChanged(ref upload, value);
		}

		private int download;
		public int Download
		{
			get => download;
			protected set => this.RaiseAndSetIfChanged(ref download, value);
		}

		[JsonConstructor]
		public LimitRule(int upload, int download, RuleSourceValue sourceValue, bool isRegex, string target, int order, bool active, Guid ruleId = default)
			: base(sourceValue, isRegex, target, order, active, ruleId)
		{

			Upload = upload;
			Download = download;
		}

		public override void Apply(Device device)
		{
			if (Match(device) == false)
				return;

			device.ResetState();
			device.Redirect();
			device.SetUploadCap(Upload);
			device.SetDownloadCap(Download);
		}

		public override string ToString()
		{
			return $"Order:{Order} - Apply On:{SourceValue} - Type:{Action} - Target:{Target} - U:{Upload}Kb/s D:{Download}Kb/s - Regex:{IsRegex} - Status:{(Active ? "Enabled" : "Disabled")}";
		}
	}
}