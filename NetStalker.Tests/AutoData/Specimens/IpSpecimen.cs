using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
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
				return IPAddress.Parse(Tools.GetRandomIpAddress());
			}

			return new NoSpecimen();
		}
	}
}