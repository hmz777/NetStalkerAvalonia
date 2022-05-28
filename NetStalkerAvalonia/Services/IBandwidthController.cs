using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IBandwidthController
    {
        void Limit(Device device, int download, int upload);
        void UnlockLimitation(Device device);
        void UnlockDownloadLimitation(Device device);
        void UnlockUploadLimitation(Device device);
    }
}