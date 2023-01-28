using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.Reflection;

namespace NetStalkerAvalonia.Converters
{
	public class RuleStatusToImageConverter : IValueConverter
	{
		public static readonly RuleStatusToImageConverter Instance = new();
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is null)
				return null;

			var status = (bool)value;
			var iconName = status ? "ok" : "error";

			var imageConverter = ImagePathToImageConverter.Instance;

			return imageConverter.Convert($"Assets/StatusMessageIcons/{iconName}.png", typeof(Bitmap), parameter, culture);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}