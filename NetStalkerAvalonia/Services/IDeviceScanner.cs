using System;

namespace NetStalkerAvalonia.Services;

public interface IDeviceScanner : IDisposable
{
    void Scan();
    void Stop();
}