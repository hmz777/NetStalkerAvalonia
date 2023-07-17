using System;
using System.Threading.Tasks;
using NetStalkerAvalonia.Core.Models;

namespace NetStalkerAvalonia.Core.Services
{
    public interface IDeviceTypeIdentifier : IDisposable
    {
        void IdentifyDevice(Device device);
    }
}
