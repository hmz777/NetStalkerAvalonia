using System;

namespace NetStalkerAvalonia.Core.Services;

public interface IDeviceScanner : IService, IDisposable
{
    void Scan();
    void Refresh();
    void Stop();
}