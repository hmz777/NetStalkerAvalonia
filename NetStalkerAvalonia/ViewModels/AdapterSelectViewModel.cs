using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Win32;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using ReactiveUI;
using WiFiAdapter = Windows.Devices.WiFi.WiFiAdapter;

namespace NetStalkerAvalonia.ViewModels;

public class AdapterSelectViewModel : ViewModelBase
{
    #region Services

    private readonly IAppLockService _appLockService;

    #endregion

    #region Members

    private List<NetworkInterface> networkInterfaces = new();

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

    public ReactiveCommand<Window, Unit> Accept { get; set; }
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

    public AdapterSelectViewModel()
    {

    }

    public AdapterSelectViewModel(IAppLockService appLockService = null!)
    {
        #region Populate network data

        GetNics();

        #endregion

        #region Command wiring

        var canAcceptExecute =
            this.WhenAnyValue(x => 
                x.GatewayIp, x => x.DriverVersion,
        (gatewayIp,driver) => 
                              string.IsNullOrEmpty(gatewayIp) == false && 
                              string.IsNullOrEmpty(driver) == false && driver != "NAN");

        Accept = ReactiveCommand.Create((Window window) => { window.Close(); }, canAcceptExecute);

        Exit = ReactiveCommand.Create(Tools.ExitApp);

        #endregion

        #region Selected Interface Event Wiring

        this.WhenAnyValue(x => x.SelectedItem)
            .Select(ReactToAdapterSelection)
            .Subscribe();

        #endregion
        
        CheckDriverAndGetVersion();

        #region App Lock

        _appLockService = Tools.ResolveIfNull<IAppLockService>(null!);

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
        }
        catch (Exception e)
        {
            return Unit.Default;
        }

        GetAdapterName();
        GetNicType();
        GetHostInfo();
        GetGatewayInfo();

        // Host has an IPV4 ip
        if (HostInfo.HostIp!.AddressFamily == AddressFamily.InterNetwork)
        {
            GetIpv4SubnetMask();
            GetNetworkClass();
        }

        await GetNetworkWifiSsidAsync();

        return Unit.Default;
    }

    private void GetAdapterName()
    {
        HostInfo.NetworkAdapterName = SelectedInterface?.Name ?? "NAN";
    }

    private void GetNicType()
    {
        NicType = SelectedInterface!.NetworkInterfaceType.ToString() ?? "Not selected";
    }

    private void GetHostInfo()
    {
        foreach (var address in SelectedInterface!.GetIPProperties().UnicastAddresses)
        {
            if (address.Address.AddressFamily == AddressFamily.InterNetwork)
            {
                HostInfo.HostIp = address.Address;
                IpAddress = HostInfo.HostIp.ToString();
                break;
            }
        }

        HostInfo.HostMac = SelectedInterface!.GetPhysicalAddress();
        MacAddress = HostInfo.HostMac.ToString();
    }

    private void GetGatewayInfo()
    {
        HostInfo.GatewayIp = SelectedInterface!
            .GetIPProperties()
            .GatewayAddresses
            .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
            .First()
            .Address;

        GatewayIp = HostInfo.GatewayIp.ToString();
    }

    private void GetIpv4SubnetMask()
    {
        foreach (var address in SelectedInterface!.GetIPProperties().UnicastAddresses)
        {
            if (address.Address.AddressFamily == AddressFamily.InterNetwork)
            {
                HostInfo.SubnetMask = address.IPv4Mask;
                return;
            }
        }
    }

    private void GetNetworkClass()
    {
        var classIndicator = Regex
            .Matches(HostInfo.SubnetMask!.ToString(), "255")
            .Count;

        switch (classIndicator)
        {
            case 1:
                HostInfo.NetworkClass = NetworkClass.A;
                break;
            case 2:
                HostInfo.NetworkClass = NetworkClass.B;
                break;
            case 3:
                HostInfo.NetworkClass = NetworkClass.C;
                break;
            default:
                throw new Exception("Invalid or not implemented network size.");
        }
    }

    private async Task GetNetworkWifiSsidAsync()
    {
        try
        {
            if (await WiFiAdapter.RequestAccessAsync() == WiFiAccessStatus.Allowed)
            {
                if (SelectedInterface!.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    var wifiDevices = await DeviceInformation
                        .FindAllAsync(WiFiAdapter.GetDeviceSelector());

                    if (wifiDevices.Count == 0)
                        return;

                    var wifi = await WiFiAdapter.FromIdAsync(wifiDevices[0].Id);
                    var profile = await wifi.NetworkAdapter.GetConnectedProfileAsync();

                    NetworkSsid = profile.GetNetworkNames().FirstOrDefault() ?? "NAN";
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private void CheckDriverAndGetVersion()
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

                    DriverVersion = version ?? "NAN";
                }
            }
        }
    }

    private void GetNics()
    {
        foreach (var net in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (net.OperationalStatus == OperationalStatus.Up &&
                net.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                networkInterfaces.Add(net);
            }
        }
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