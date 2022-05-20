using Splat;

namespace NetStalkerAvalonia.Helpers;

public class Tools
{
    public static T ResolveIfNull<T>(T dependency)
    {
        if (dependency != null)
            return dependency;

        return Locator.Current.GetService<T>()!;
    }
}