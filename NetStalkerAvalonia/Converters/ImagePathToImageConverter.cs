using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Splat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Converters
{
	public class ImagePathToImageConverter : IValueConverter
	{
		public readonly static ImagePathToImageConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is null)
			{
				return null;
			}

			var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
			return new Bitmap(assets.Open(
			new Uri(
				string.Format("avares://{0}{1}",
					Assembly.GetExecutingAssembly().GetName().Name,
					value))));
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
