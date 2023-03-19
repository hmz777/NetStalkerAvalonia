using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Models;
using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Services.Implementations.DeviceNameResolving
{
	public class DeviceNameResolver : IDeviceNameResolver
	{
		private const string _deviceNamesResource = "Devices.json";

		private readonly IFileSystem fileSystem;

		public DeviceNameResolver(IFileSystem fileSystem)
		{
			this.fileSystem = fileSystem;

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
				using (var stream = fileSystem.FileStream.New(_deviceNamesResource, FileMode.OpenOrCreate))
				{
					var deviceNameModels = JsonSerializer
						.Deserialize<List<DeviceNameModel>>(stream);

					DevicesNames = deviceNameModels ?? new List<DeviceNameModel>();
				}
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), this.GetType(), e.Message);
			}
		}

		public void SaveDeviceNamesAsync(IEnumerable<Device> devices)
		{
			try
			{
				var deviceNamesJson = JsonSerializer
					.Serialize(devices
						.Where(d => d.HasFriendlyName)
						.Select(device => new DeviceNameModel(device.Mac.ToString(), device.Name)));

				fileSystem.File.WriteAllText(_deviceNamesResource, deviceNamesJson);
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), this.GetType(), e.Message);
			}
		}

		public void ClearDeviceNames()
		{
			if (fileSystem.File.Exists(_deviceNamesResource))
			{
				fileSystem.File.Delete(_deviceNamesResource);
			}
		}
	}
}