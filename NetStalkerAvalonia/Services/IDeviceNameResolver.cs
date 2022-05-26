using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceNameResolver
    {
        void GetDeviceName(Device device);
    }
}