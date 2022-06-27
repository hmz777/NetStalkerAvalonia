using NetStalkerAvalonia.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Services.Implementations.DeviceNameResolving
{
    public class DeviceNameResolver : IDeviceNameResolver
    {
        public async Task GetDeviceNameAsync(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            try
            {
                var ipHostEntry = await Dns.GetHostEntryAsync(device.Ip!);
                device.SetFriendlyName(ipHostEntry.HostName);
            }
            catch
            {
                device.SetFriendlyName(device.Ip!.ToString());
            }
        }
    }
}