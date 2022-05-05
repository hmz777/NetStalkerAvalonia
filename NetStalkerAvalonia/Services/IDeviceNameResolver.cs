using System.Net;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceNameResolver
    {
        string GetDeviceName(IPAddress ip);
    }
}