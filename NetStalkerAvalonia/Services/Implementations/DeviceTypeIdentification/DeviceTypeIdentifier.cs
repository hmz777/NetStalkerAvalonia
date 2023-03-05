using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using Serilog;
using Splat;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Services.Implementations.DeviceTypeIdentification
{
	public class DeviceTypeIdentifier : IDeviceTypeIdentifier
	{
		#region Members

		private readonly IFileSystem fileSystem;

		private readonly CancellationTokenSource _cancellationTokenSource;
		private Task? _serviceTask;

		private bool _isStarted;

		private readonly string? macLookupServiceUri = "https://api.macvendors.com/{Mac}";
		private readonly HttpClient? _client;

		private const string _localVendorDatabase = "manuf.txt";
		private string _localVendorDb;

		// The queue is to prevent flooding the mac lookup service with requests
		private Queue<Device> _identificationQueue;

		#endregion

		#region Constructor

		public DeviceTypeIdentifier(IFileSystem fileSystem, HttpClient client = null!)
		{
			this.fileSystem = fileSystem;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				// Check if HttpClient is registered with the DI container, which indicates that an API token is set
				_client = Tools.ResolveIfNull(client);
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ServiceResolveError,
					e.Message);
			}

			_identificationQueue = new Queue<Device>();
		}

		#endregion

		#region Internal

		private async Task StartIdentifierAsync()
		{
			// Load the vendor db into memory
			_localVendorDb = await fileSystem.File.ReadAllTextAsync(_localVendorDatabase, _cancellationTokenSource.Token);

			while (_cancellationTokenSource.IsCancellationRequested == false)
			{
				try
				{
					if (_identificationQueue.TryDequeue(out var deviceToIdentify))
					{
						string data = string.Empty;

						if (string.IsNullOrWhiteSpace(Config.AppSettings.VendorApiTokenSetting) == false)
						{
							data = await ResolveVendorRemotelyAsync(deviceToIdentify.Mac);
						}

						// If we got an empty string we try to get the vendor from the local db
						if (string.IsNullOrWhiteSpace(data))
						{
							data = ResolveVendorLocally(deviceToIdentify.Mac);
						}

						deviceToIdentify.SetVendor(string.IsNullOrWhiteSpace(data) ? "Not found" : data);
					}
				}
				catch (Exception e)
				{
					Log.Error(LogMessageTemplates.ExceptionTemplate,
						e.GetType(), this.GetType(), e.Message);
				}

				await Task.Delay(1000);
			}
		}

		private string ResolveVendorLocally(PhysicalAddress mac)
		{
			var match = Regex.Match(_localVendorDb, @$"({mac.ToOuiMac()})\t(\w+)\t(.*)");

			return match.Value;
		}

		private async Task<string> ResolveVendorRemotelyAsync(PhysicalAddress mac)
		{
			try
			{
				var serviceUri = macLookupServiceUri!
							.Replace("{Mac}", mac.ToOuiMac());

				var data = await _client!
					.GetStringAsync(serviceUri);

				return data;
			}
			catch
			{
			}

			return string.Empty;
		}

		#endregion

		#region API

		public void IdentifyDevice(Device device)
		{
			if (_isStarted == false)
			{
				_serviceTask = StartIdentifierAsync();
				_isStarted = true;
			}

			if (_identificationQueue.Contains(device) == false)
				_identificationQueue.Enqueue(device);
		}

		public void Dispose()
		{
			_serviceTask?.Wait();
			_serviceTask?.Dispose();
			_isStarted = false;
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}

		#endregion
	}
}