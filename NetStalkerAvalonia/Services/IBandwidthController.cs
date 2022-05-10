using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IBandwidthController
    {
        void Limit(Device device, int download, int upload);
    }
}