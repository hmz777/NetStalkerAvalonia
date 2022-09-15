using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetStalkerAvalonia.Helpers;
using Serilog;
using SharpPcap;

namespace NetStalkerAvalonia.Services.Implementations.DeviceNameResolving
{
    public class DeviceNameResolver : IDeviceNameResolver
    {
        private string _deviceNamesResource = "Devices.json";

        public DeviceNameResolver()
        {
            DevicesNames = new List<DeviceNameModel>();
            LoadDevicesNames();
        }

        public List<DeviceNameModel> DevicesNames { get; private set; }

        public async Task ResolveDeviceNameAsync(Device device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            var deviceFriendlyName = DevicesNames
                .Where(dn => dn.Mac.Equals(device.Mac))
                .FirstOrDefault();

            if (deviceFriendlyName != null)
            {
                device.SetFriendlyName(deviceFriendlyName.Name);
            }
            else
            {
                try
                {
                    var ipHostEntry = await Dns.GetHostEntryAsync(device.Ip!);
                    device.SetFriendlyName(ipHostEntry.HostName);
                }
                catch
                {
                    device.SetFriendlyName(device.Ip.ToString());
                }
            }
        }

        public void LoadDevicesNames()
        {
            try
            {
                using (var stream = new FileStream(_deviceNamesResource, FileMode.OpenOrCreate))
                {
                    var deviceNameModels = JsonSerializer
                        .Deserialize<List<DeviceNameModel>>(stream);

                    DevicesNames = deviceNameModels == null ? new List<DeviceNameModel>() : deviceNameModels;
                }
            }
            catch (Exception e)
            {
                Log.Error(LogMessageTemplates.ExceptionTemplate,
                    e.GetType(), this.GetType(), e.Message);
            }
        }

        public async Task SaveDeviceNamesAsync(List<Device> devices, CancellationToken cancellationToken = default)
        {
            try
            {
                var deviceNamesJson = JsonSerializer
                    .Serialize(devices.Select(device => new DeviceNameModel(device.Mac, device.Name)));

                await File.WriteAllTextAsync(_deviceNamesResource, deviceNamesJson, cancellationToken);
            }
            catch (Exception e)
            {
                Log.Error(LogMessageTemplates.ExceptionTemplate,
                    e.GetType(), this.GetType(), e.Message);
            }
        }

        public void ClearDeviceNames()
        {
            File.Delete(_deviceNamesResource);
        }
    }
}