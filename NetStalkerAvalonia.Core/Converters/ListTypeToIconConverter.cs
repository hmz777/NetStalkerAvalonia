using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NetStalkerAvalonia.Core.Models;
using System;
using System.Globalization;
using System.Reflection;

namespace NetStalkerAvalonia.Core.Converters
{
    public class ListTypeToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(value);

            var type = (DeviceType)value;

            var fileName = $"/Assets/DeviceListIcons/{type.ToString().ToLower()}.png";

            return new Bitmap(AssetLoader.Open(
                     new Uri(string.Format("avares://{0}{1}", Assembly.GetExecutingAssembly().GetName().Name, fileName))));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}