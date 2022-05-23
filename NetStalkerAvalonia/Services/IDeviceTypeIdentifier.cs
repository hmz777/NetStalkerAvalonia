using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceTypeIdentifier
    {
        DeviceType IdentifyDevice(Device device);
    }
}
