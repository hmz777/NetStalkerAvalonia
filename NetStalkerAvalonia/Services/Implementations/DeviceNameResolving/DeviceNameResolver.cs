using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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

            try
            {
                var deviceFriendlyName = DevicesNames
                    .Where(dn => PhysicalAddress.Parse(dn.Mac).Equals(device.Mac))
                    .FirstOrDefault();

                if (deviceFriendlyName != null)
                {
                    device.SetFriendlyName(deviceFriendlyName.Name);
                }
                else
                {
                    var ipHostEntry = await Dns.GetHostEntryAsync(device.Ip!);
                    device.SetFriendlyName(ipHostEntry.HostName, true);
                }
            }
            catch
            {
                // It doesn't matter if we specify the second optional parameter or not
                device.SetFriendlyName(null!);
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

        public async Task SaveDeviceNamesAsync(IEnumerable<Device> devices,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var deviceNamesJson = JsonSerializer
                    .Serialize(devices
                        .Where(d => d.HasFriendlyName)
                        .Select(device => new DeviceNameModel(device.Mac.ToString(), device.Name)));

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
            if (File.Exists(_deviceNamesResource))
            {
                File.Delete(_deviceNamesResource);
            }
        }
    }
}