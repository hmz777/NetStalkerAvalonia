using NetStalkerAvalonia.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.ViewModels.InteractionViewModels
{
	public class AddUpdateRuleModel
	{
		public RuleAction Action { get; }
		public RuleSourceValue SourceValue { get; }
		public string Target { get; }
		public bool IsRegex { get; }
		public int Order { get; }
		public bool Active { get; }
		public int Upload { get; }
		public int Download { get; }

		public AddUpdateRuleModel(RuleAction action, RuleSourceValue sourceValue, string target, bool isRegex, int order, bool active, int upload, int download)
		{
			Action = action;
			SourceValue = sourceValue;
			Target = target ?? throw new ArgumentNullException(nameof(target));
			IsRegex = isRegex;
			Order = order > 0 ? order : throw new ArgumentException("Order can't be negative or zero", nameof(order));
			Active = active;
			Upload = upload >= 0 ? upload : throw new ArgumentException("Upload can't be negative", nameof(upload)); ;
			Download = download >= 0 ? download : throw new ArgumentException("Download can't be negative", nameof(download)); ;
		}
	}
}
