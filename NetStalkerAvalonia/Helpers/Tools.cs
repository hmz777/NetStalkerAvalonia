using System;
using System.Linq;
using System.Net;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using SharpPcap;
using SharpPcap.LibPcap;
using Splat;

namespace NetStalkerAvalonia.Helpers;

public class Tools
{
    public static T ResolveIfNull<T>(T dependency)
    {
        if (dependency != null)
            return dependency;

        dependency = Locator.Current.GetService<T>()!;

        if (dependency == null &&
            OptionalFeatures.AvailableFeatures.Contains(typeof(T)) == false)
        {
            // Only throw on non-optional features
            throw new Exception(string.Format("The dependency locator returned null of type {0}.", typeof(T)));
        }

        return dependency;
    }

    public static string GetRootIp(IPAddress ipaddress, NetworkClass networkClass)
    {
        ArgumentNullException.ThrowIfNull(ipaddress);

        var ipaddressstring = ipaddress.ToString();

        switch (networkClass)
        {
            case NetworkClass.A:
                return ipaddressstring
                    .Substring(0,
                        ipaddressstring
                            .IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1);
            case NetworkClass.B:
                return ipaddressstring
                    .Substring(0,
                        ipaddressstring
                            .IndexOf(".", 4, StringComparison.InvariantCultureIgnoreCase) + 1);
            case NetworkClass.C:
                return ipaddressstring
                    .Substring(0,
                        ipaddressstring
                            .LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1);
            default:
                throw new ArgumentOutOfRangeException(nameof(networkClass), networkClass, null);
        }
    }

    public static void ResolveGateway()
    {
        var adapterName = (from devicex in LibPcapLiveDeviceList.Instance
            where devicex.Interface.FriendlyName == HostInfo.NetworkAdapterName
            select devicex).ToList()[0].Name;

        using (var device = LibPcapLiveDeviceList.New()[adapterName])
        {
            var gatewayArp = new ARP(device);
            var gatewayMac = gatewayArp.Resolve(HostInfo.GatewayIp);

            if (gatewayMac == null)
            {
                throw new Exception("Couldn't resolve gateway mac address.");
            }

            HostInfo.GatewayMac = gatewayMac;
        }
    }

    public static void ExitApp()
    {
        var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        app?.Shutdown();
    }
}