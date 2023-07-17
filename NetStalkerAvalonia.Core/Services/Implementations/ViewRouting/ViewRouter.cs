using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Services.Implementations.ViewRouting
{
	public class ViewRouter : IRouter
	{
		public RoutingState Router { get; set; }

		public ViewRouter()
		{
			Router = new RoutingState();
		}
	}
}