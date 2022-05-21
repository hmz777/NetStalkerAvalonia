using System;
using System.Net;
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

    public static string GetRootIp(IPAddress ipaddress)
    {
        ArgumentNullException.ThrowIfNull(ipaddress);

        var ipaddressstring = ipaddress.ToString();
        return ipaddressstring
            .Substring(0, ipaddressstring.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1);
    }
}