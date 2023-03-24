using NetStalkerAvalonia.Core.Services.Implementations.ErrorHandling;
using NetStalkerAvalonia.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class StatusMessageCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IStatusMessageService), typeof(StatusMessageService)));
			fixture.Register(() => new StatusMessageService());
		}
	}
}