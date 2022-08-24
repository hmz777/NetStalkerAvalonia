using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using Serilog;
using IValueConverter = Avalonia.Data.Converters.IValueConverter;

namespace NetStalkerAvalonia.Converters;

public class StatusMessageTypeToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Enum.TryParse(value?.ToString(), out MessageType messageType) == false)
        {
            return null!;
        }

        var source = MessageTypeToImageName(messageType);

        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

        return new Bitmap(assets.Open(
            new Uri(
                string.Format("avares://{0}/{1}/{2}.png",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    "Assets/StatusMessageIcons",
                    source))));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private string? MessageTypeToImageName(MessageType messageType) => messageType switch
    {
        MessageType.Error => "error",
        MessageType.Information => "info",
        MessageType.Success => "ok",
        MessageType.Warning => "warning",
        _ => null
    };
}