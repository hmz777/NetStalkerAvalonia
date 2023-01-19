using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Rules.Implementations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Services
{
	public interface IRuleService : IService
	{
		/// <summary>
		/// Apply the rules that match the provided device.
		/// </summary>
		/// <remarks>
		/// If multiple rules match the same device, the last one in terms of order is applied.
		/// </remarks>
		/// <param name="device"></param>
		public void ApplyIfMatch(Device device);
		public void AddBlockingRule(BlockRule blockRule);
		public void AddRedirectingRule(RedirectRule redirectRule);
		public void AddLimitingRule(LimitRule limitRule);

		public IEnumerable<IRule> Rules { get; }
	}
}