using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IBlockerRedirector : IService, IDisposable
    {
        void Block(PhysicalAddress mac);
        void UnBlock(PhysicalAddress mac);
        void Redirect(PhysicalAddress mac);
        void UnRedirect(PhysicalAddress mac);
        void Limit(PhysicalAddress mac, int download, int upload);
        void LimitDownload(PhysicalAddress mac, int download);
        void LimitUpload(PhysicalAddress mac, int upload);
        void ClearLimits(PhysicalAddress mac);
        void ClearDownload(PhysicalAddress mac);
        void ClearUpload(PhysicalAddress mac);
    }
}