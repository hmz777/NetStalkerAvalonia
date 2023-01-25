using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Rules
{
	public interface IRule
	{
		public Guid RuleId { get; }
		public string Target { get; }
		public int Order { get; }
		public void Activate();
		public void Deactivate();
		public bool Match(Device device);
		public void Apply(Device device);
	}
}