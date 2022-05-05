using NetStalkerAvalonia.Models;
using System.Net;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceTypeIdentifier
    {
        DeviceType IdentifyDevice(IPAddress ip);
    }
}
