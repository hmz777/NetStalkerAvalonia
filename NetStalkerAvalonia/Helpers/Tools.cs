using System;
using System.Net;
using NetStalkerAvalonia.Models;
using Splat;

namespace NetStalkerAvalonia.Helpers;

public class Tools
{
    public static T ResolveIfNull<T>(T dependency)
    {
        if (dependency != null)
            return dependency;

        dependency = Locator.Current.GetService<T>()!;

        if (dependency == null)
        {
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
}