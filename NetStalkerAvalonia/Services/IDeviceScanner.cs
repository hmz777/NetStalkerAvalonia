using System;

namespace NetStalkerAvalonia.Services;

public interface IDeviceScanner : IDisposable
{
    void Scan();
    void Refresh();
    void Stop();
}