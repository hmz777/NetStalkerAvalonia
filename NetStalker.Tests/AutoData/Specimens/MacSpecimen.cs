using AutoFixture.Kernel;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata;

namespace NetStalker.Tests.AutoData.Specimens
{
	public class MacSpecimen : ISpecimenBuilder
	{
		public object Create(object request, ISpecimenContext context)
		{
			if (request is ParameterInfo parameter && parameter.ParameterType == typeof(PhysicalAddress) && parameter.Name == nameof(Device.Mac).ToLower())
			{
				return Tools.GetRandomMacAddress();
			}

			return new NoSpecimen();
		}
	}
}
