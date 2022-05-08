using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NetStalkerAvalonia.Compairers
{
    public class DeviceEqualityComparer : IEqualityComparer<Device>
    {
        public bool Equals(Device? x, Device? y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            return x.Mac.Equals(y.Mac, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Device obj)
        {
            return obj.Mac.GetHashCode();
        }
    }
}