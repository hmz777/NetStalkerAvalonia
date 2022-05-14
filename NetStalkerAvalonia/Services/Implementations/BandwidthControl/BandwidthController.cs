using NetStalkerAvalonia.Models;
using System;

namespace NetStalkerAvalonia.Services.Implementations.BandwidthControl
{
    public class BandwidthController : IBandwidthController
    {
        public void Limit(Device device, int download, int upload)
        {
            throw new NotImplementedException();
        }

        public void UnlockDownloadLimitation(Device device)
        {
            throw new NotImplementedException();
        }

        public void UnlockUploadLimitation(Device device)
        {
            throw new NotImplementedException();
        }
    }
}
