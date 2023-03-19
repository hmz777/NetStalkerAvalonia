using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetStalkerAvalonia.Core.Converters
{
    public class PhysicalAddressToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not PhysicalAddress phAddress)
                return null;

            return string.Concat(phAddress.GetAddressBytes().Select(by => string.Format("{0}-", by.ToString("X2"))).ToArray()).TrimEnd('-');
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value?.ToString()))
                return null;

            return PhysicalAddress.Parse(value.ToString());
        }
    }
}