using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IBlockerRedirector : IService, IDisposable
    {
        void Block(Device device);
        void UnBlock(Device device);
        void Redirect(Device device);
        void UnRedirect(Device device);
        void Limit(Device device, int download, int upload);
        void LimitDownload(Device device, int download);
        void LimitUpload(Device device, int upload);
        void ClearLimits(Device device);
        void ClearDownload(Device device);
        void ClearUpload(Device device);
    }
}