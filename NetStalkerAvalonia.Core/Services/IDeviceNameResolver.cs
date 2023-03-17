using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceNameResolver
	{
		public List<DeviceNameModel> DevicesNames { get; }
		Task ResolveDeviceNameAsync(Device device);
		void SaveDeviceNamesAsync(IEnumerable<Device> devices);
		void ClearDeviceNames();
	}
}