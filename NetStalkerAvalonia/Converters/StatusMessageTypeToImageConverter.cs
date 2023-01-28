using Avalonia.Media.Imaging;
using NetStalkerAvalonia.Models;
using System;
using System.Globalization;
using IValueConverter = Avalonia.Data.Converters.IValueConverter;

namespace NetStalkerAvalonia.Converters;

public class StatusMessageTypeToImageConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (Enum.TryParse(value?.ToString(), out MessageType messageType) == false)
		{
			return null!;
		}

		var source = MessageTypeToImageName(messageType);

		var imageConverter = ImagePathToImageConverter.Instance;

		return imageConverter.Convert($"Assets/StatusMessageIcons/{source}.png", typeof(Bitmap), parameter, culture);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
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