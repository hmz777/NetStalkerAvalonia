using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services.Implementations.BandwidthControl
{
    public class BandwidthController : IBandwidthController
    {
        public void Limit(Device device, int download, int upload)
        {
            device.SetDownload(download);
            device.SetUpload(upload);
        }

        public void UnlockLimitation(Device device)
        {
            device.SetDownload(0);
            device.SetUpload(0);
        }

        public void UnlockDownloadLimitation(Device device)
        {
            device.SetDownload(0);
        }

        public void UnlockUploadLimitation(Device device)
        {
            device.SetUpload(0);
        }
    }
}