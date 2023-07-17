using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetStalkerAvalonia.Core.Models;
using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;

namespace NetStalkerAvalonia.Core.Services
{
    public interface IDeviceNameResolver
	{
		public List<DeviceNameModel> DevicesNames { get; }
		Task ResolveDeviceNameAsync(Device device);
		void SaveDeviceNamesAsync(IEnumerable<Device> devices);
		void ClearDeviceNames();
	}
}