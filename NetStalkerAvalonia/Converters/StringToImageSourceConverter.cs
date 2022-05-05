using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.Reflection;

namespace NetStalkerAvalonia.Converters
{
    public class StringToImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var source = value?.ToString();

            if (source == null)
                return null;

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            return new Bitmap(assets.Open(
                new Uri(
                    string.Format("avares://{0}{1}", Assembly.GetExecutingAssembly().GetName().Name, source))));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}