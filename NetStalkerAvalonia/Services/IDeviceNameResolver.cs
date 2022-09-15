using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceNameResolver
    {
        public List<DeviceNameModel> DevicesNames { get; }
        Task ResolveDeviceNameAsync(Device device);
        Task SaveDeviceNamesAsync(List<Device> devices, CancellationToken cancellationToken = default);
        void ClearDeviceNames();
    }
}