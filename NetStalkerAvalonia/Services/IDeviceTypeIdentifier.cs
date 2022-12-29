using System;
using System.Threading.Tasks;
using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceTypeIdentifier : IDisposable
    {
        void IdentifyDevice(Device device);
    }
}
