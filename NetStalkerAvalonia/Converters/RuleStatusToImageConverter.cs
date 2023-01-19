using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Converters
{
	public class RuleStatusToImageConverter : IValueConverter
	{
		public static readonly RuleStatusToImageConverter Instance = new();
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is not null)
			{
				var status = (bool)value;

				var iconName = status ? "ok" : "error";

				var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

				return new Bitmap(assets.Open(
					new Uri(
						string.Format("avares://{0}/{1}/{2}.png",
							Assembly.GetExecutingAssembly().GetName().Name,
							"Assets/StatusMessageIcons",
							iconName))));

			}

			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
