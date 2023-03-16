using Microsoft.Win32;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using ReactiveUI;
using Serilog;
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

	public readonly PasswordViewModel _passwordViewModel;

	#endregion

	#region Members

	private readonly List<NetworkInterface> networkInterfaces = new();

	private string? _selectedItem;
	private string? _nicType;
	private string? _hostIp;
	private string? _hostMac;
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

	public string? HostIp
	{
		get => _hostIp;
		set => this.RaiseAndSetIfChanged(ref _hostIp, value);
	}

	public string? HostMac
	{
		get => _hostMac;
		set => this.RaiseAndSetIfChanged(ref _hostMac, value);
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
	public AdapterSelectViewModel(
		IPcapDeviceManager pcapDeviceManager,
		IAppLockService appLockService,
		PasswordViewModel passwordViewModel)
	{
		_pcapDeviceManager = pcapDeviceManager;
		_appLockService = appLockService;
		_passwordViewModel = passwordViewModel;

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

			var adapterName = GetAdapterName();

			if (string.IsNullOrEmpty(adapterName))
			{
				return Unit.Default;
			}

			var gatewayIp = GetGatewayIp();
			var gatewayMac = GetGatewayMac(gatewayIp, adapterName);
			var hostIp = GetHostIp();
			var hostMac = GetHostMac();
			var subnetMask = GetIpv4SubnetMask();
			var ipType = gatewayIp.AddressFamily == AddressFamily.InterNetwork ?
				IpType.Ipv4 : IpType.Ipv6;

			HostInfo.SetHostInfo(
				networkAdapterName: adapterName,
				gatewayIp: gatewayIp,
				gatewayMac: gatewayMac,
				hostIp: hostIp,
				hostMac: hostMac,
				ipType: ipType,
				subnetMask: subnetMask,
				networkClass: GetNetworkClass(subnetMask));

			GatewayIp = gatewayIp.ToString();
			HostIp = hostIp.ToString();
			HostMac = hostMac.ToString();
			NicType = GetNicType();
			NetworkSsid = await GetNetworkWifiSsidAsync();
		}
		catch (Exception e)
		{
			Log.Error(LogMessageTemplates.ExceptionTemplate, e.GetType(), e.Message);

			return Unit.Default;
		}

		return Unit.Default;
	}

	private string? GetAdapterName()
	{
		return SelectedInterface?.Name;
	}

	private string GetNicType()
	{
		return SelectedInterface!.NetworkInterfaceType.ToString() ?? "Not selected";
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
			.FirstOrDefault()?
			.Address;
	}

	public PhysicalAddress? GetGatewayMac(IPAddress gatewayIp, string? adapterName)
	{
		ArgumentNullException.ThrowIfNull(gatewayIp, nameof(gatewayIp));

		using var device = _pcapDeviceManager.CreateDevice("arp", null, 20, adapterName);

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
		ArgumentNullException.ThrowIfNull(subnetMask, nameof(subnetMask));

		var classIndicator = Regex
			.Matches(subnetMask.ToString(), "255")
			.Count;

		switch (classIndicator)
		{
			case 1: return NetworkClass.A;
			case 2: return NetworkClass.B;
			case 3: return NetworkClass.C;
			default:
				throw new Exception("Invalid network class");
		}
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
		HostIp = default;
		HostMac = default;
		GatewayIp = default;
		NetworkSsid = default;
	}

	#endregion
}