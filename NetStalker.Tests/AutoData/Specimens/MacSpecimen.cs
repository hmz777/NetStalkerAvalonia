using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Models;
using System.Net.NetworkInformation;
using System.Reflection;

namespace NetStalker.Tests.AutoData.Specimens
{
	public class MacSpecimen : ISpecimenBuilder
	{
		public object Create(object request, ISpecimenContext context)
		{
			if (request is ParameterInfo parameter && parameter.ParameterType == typeof(PhysicalAddress) && parameter.Name == nameof(Device.Mac).ToLower())
			{
				return DataHelpers.GetRandomMacAddress();
			}

			return new NoSpecimen();
		}
	}
}
