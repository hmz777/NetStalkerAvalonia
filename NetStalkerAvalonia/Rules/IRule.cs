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
		public int Order { get; set; }
		public void Activate();
		public void Deactivate();
		public bool Match(Device device);
		public void Apply(Device device);
	}
}