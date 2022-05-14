using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceNameResolver
    {
        string GetDeviceName(Device device);
    }
}