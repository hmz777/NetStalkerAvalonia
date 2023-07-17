using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Models;
using System.Net;
using System.Reflection;

namespace NetStalker.Tests.AutoData.Specimens
{
	public class IpSpecimen : ISpecimenBuilder
	{
		public object Create(object request, ISpecimenContext context)
		{
			if (request is ParameterInfo parameter && parameter.ParameterType == typeof(IPAddress) && parameter.Name == nameof(Device.Ip).ToLower())
			{
				return DataHelpers.GetRandomIpAddress();
			}

			return new NoSpecimen();
		}
	}
}