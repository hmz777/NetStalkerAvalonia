using System;

namespace NetStalkerAvalonia.Services;

public interface IDeviceScanner : IService, IDisposable
{
    void Scan();
    void Refresh();
    void Stop();
}