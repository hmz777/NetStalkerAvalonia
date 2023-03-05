using Microsoft.Win32;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using ReactiveUI;
using SharpPcap;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;

namespace NetStalkerAvalonia.ViewModels;

public class AdapterSelectViewModel : ViewModelBase
{
	#region Services

	private readonly IPcapDeviceManager _pcapDeviceManager;
	private readonly IAppLockService _appLockService;

	#endregion

	#region Members

	private readonly List<NetworkInterface> networkInterfaces = new();

	private string? _selectedItem;
	private string? _nicType;
	private string? _ipAddress;
	private string? _macAddress;
	private string? _gatewayIp;
	private string? _networkSsid;
	private string? _driverVersion;

	#endregion

	#region Properties

	#region Commands

	public ReactiveCommand<Unit, Unit> Accept { get; set; }
	public ReactiveCommand<Unit, Unit> Exit { get; set; }

	#endregion

	#region UI Data

	public NetworkInterface? SelectedInterface { get; set; }

	public List<string> ComboBoxInterfaceData =>
		networkInterfaces.Select(nic => nic.Name).ToList();

	public string? SelectedItem
	{
		get => _selectedItem;
		set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
	}

	public string? NicType
	{
		get => _nicType;
		set => this.RaiseAndSetIfChanged(ref _nicType, value);
	}

	public string? IpAddress
	{
		get => _ipAddress;
		set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
	}

	public string? MacAddress
	{
		get => _macAddress;
		set => this.RaiseAndSetIfChanged(ref _macAddress, value);
	}

	public string? GatewayIp
	{
		get => _gatewayIp;
		set => this.RaiseAndSetIfChanged(ref _gatewayIp, value);
	}

	public string? NetworkSsid
	{
		get => _networkSsid;
		set => this.RaiseAndSetIfChanged(ref _networkSsid, value);
	}

	public string? DriverVersion
	{
		get => _driverVersion;
		set => this.RaiseAndSetIfChanged(ref _driverVersion, value);
	}

	private readonly ObservableAsPropertyHelper<bool> _isAppLocked;
	public bool IsAppLocked => _isAppLocked.Value;

	#endregion

	#endregion

	#region Constructor

#if DEBUG

	public AdapterSelectViewModel()
	{

	}

#endif

	[Splat.DependencyInjectionConstructor]
	public AdapterSelectViewModel(IPcapDeviceManager pcapDeviceManager, IAppLockService appLockService)
	{
		_pcapDeviceManager = pcapDeviceManager;
		_appLockService = appLockService;

		#region Populate data

		networkInterfaces = GetNics();
		DriverVersion = CheckDriverAndGetVersion();

		#endregion

		#region Command wiring

		var canAcceptExecute =
			this.WhenAnyValue(x =>
				x.GatewayIp, x => x.DriverVersion,
		(gatewayIp, driver) =>
							  string.IsNullOrEmpty(gatewayIp) == false &&
							  string.IsNullOrEmpty(driver) == false && driver != "NAN");

		// This command does nothing but validating that settings are valid and to subscribe to it so we can show the main window
		Accept = ReactiveCommand.Create(() => { }, canAcceptExecute);

		Exit = ReactiveCommand.Create(Tools.ExitApp);

		#endregion

		#region Selected Interface Event Wiring

		this.WhenAnyValue(x => x.SelectedItem)
			.Select(ReactToAdapterSelection)
			.Subscribe();

		#endregion

		#region App Lock

		_isAppLocked = this.WhenAnyValue(x => x._appLockService!.IsLocked)
					  .ToProperty(this, x => x.IsAppLocked);

		#endregion
	}

	#endregion

	#region Tools

	private async Task<Unit> ReactToAdapterSelection(string? item)
	{
		ClearAll();

		try
		{
			SelectedInterface = networkInterfaces
				.Where(nic => nic.Name == item)
				.FirstOrDefault();

			if (SelectedInterface == null)
				return Unit.Default;

			var gatewayIp = GetGatewayIp();
			var subnetMask = GetIpv4SubnetMask();
			var ipType = gatewayIp.AddressFamily == AddressFamily.InterNetwork ?
				IpType.Ipv4 : IpType.Ipv6;

			HostInfo.SetHostInfo(
				networkAdapterName: GetAdapterName(),
				gatewayIp: gatewayIp,
				gatewayMac: GetGatewayMac(gatewayIp),
				hostIp: GetHostIp(),
				hostMac: GetHostMac(),
				ipType: ipType,
				subnetMask: subnetMask,
				networkClass: GetNetworkClass(subnetMask));

			NetworkSsid = await GetNetworkWifiSsidAsync();
		}
		catch
		{
			return Unit.Default;
		}

		return Unit.Default;
	}

	private string? GetAdapterName()
	{
		return SelectedInterface?.Name;
	}

	private void GetNicType()
	{
		NicType = SelectedInterface!.NetworkInterfaceType.ToString() ?? "Not selected";
	}

	private IPAddress? GetHostIp()
	{
		foreach (var address in SelectedInterface!.GetIPProperties().UnicastAddresses)
		{
			if (address.Address.AddressFamily == AddressFamily.InterNetwork)
			{
				return address.Address;

			}
		}

		return default;
	}

	private PhysicalAddress? GetHostMac()
	{
		return SelectedInterface?.GetPhysicalAddress();
	}

	private IPAddress? GetGatewayIp()
	{
		return SelectedInterface?
			.GetIPProperties()
			.GatewayAddresses
			.Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
			.First()
			.Address;
	}

	public PhysicalAddress? GetGatewayMac(IPAddress gatewayIp)
	{
		using var device = _pcapDeviceManager.CreateDevice("arp", (s, m) => { }, 20);

		var gatewayArp = device.CreateArp();
		var gatewayMac = gatewayArp.Resolve(gatewayIp);

		return gatewayMac;
	}

	private IPAddress? GetIpv4SubnetMask()
	{
		foreach (var address in SelectedInterface!.GetIPProperties().UnicastAddresses)
		{
			if (address.Address.AddressFamily == AddressFamily.InterNetwork)
			{
				return address.IPv4Mask;
			}
		}

		return default;
	}

	private NetworkClass GetNetworkClass(IPAddress subnetMask)
	{
		var classIndicator = Regex
			.Matches(subnetMask.ToString(), "255")
			.Count;

		return (NetworkClass)classIndicator;
	}

	private async Task<string?> GetNetworkWifiSsidAsync()
	{
		try
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				if (await WiFiAdapter.RequestAccessAsync() == WiFiAccessStatus.Allowed)
				{
					if (SelectedInterface!.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
					{
						var wifiDevices = await DeviceInformation
							.FindAllAsync(WiFiAdapter.GetDeviceSelector());

						if (wifiDevices.Count == 0)
							return default;

						var wifi = await WiFiAdapter.FromIdAsync(wifiDevices[0].Id);
						var profile = await wifi.NetworkAdapter.GetConnectedProfileAsync();

						return profile.GetNetworkNames().FirstOrDefault();
					}
				}
			}

			// TODO: Find a cross-platform way of getting the Wifi SSID
		}
		catch
		{
			// ignored
		}

		return default;
	}

	private string CheckDriverAndGetVersion()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var npcapRegKey = Environment.Is64BitOperatingSystem == false
		   ? @"SOFTWARE\Npcap"
		   : @"SOFTWARE\WOW6432Node\Npcap";

			using (var npcapKey = Registry.LocalMachine.OpenSubKey(npcapRegKey, false))
			{
				//Get Npcap installation path
				if (npcapKey != null)
				{
					var installationPath = npcapKey.GetValue(string.Empty) as string;

					if (!string.IsNullOrEmpty(installationPath))
					{
						var version = FileVersionInfo
							.GetVersionInfo(Path.Combine(installationPath, "NPFInstall.exe"))
							.FileVersion;

						return version ?? "NAN";
					}
				}
			}
		}

		// TODO: Implement Linux logic

		return "NAN";
	}

	private List<NetworkInterface> GetNics()
	{
		return NetworkInterface.GetAllNetworkInterfaces().Where(net =>
				net.OperationalStatus == OperationalStatus.Up &&
				net.NetworkInterfaceType != NetworkInterfaceType.Loopback).ToList();
	}

	private void ClearAll()
	{
		NicType = default;
		IpAddress = default;
		MacAddress = default;
		GatewayIp = default;
		NetworkSsid = default;
	}

	#endregion
}