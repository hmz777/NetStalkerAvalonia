using Avalonia.Data.Converters;
using NetStalkerAvalonia.Rules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Converters
{
	public class RuleSourceValueToStringConverter : IValueConverter
	{
		public readonly static RuleSourceValueToStringConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (Enum.IsDefined(typeof(RuleSourceValue), value!))
			{
				var source = (RuleSourceValue)value!;

				switch (source)
				{
					case RuleSourceValue.IPAddress:
						return "IP Address";
					case RuleSourceValue.MacAddress:
						return "MAC Address";
					case RuleSourceValue.Name:
						return "Name";
					case RuleSourceValue.Vendor:
						return "Vendor";
					default:
						break;
				}
			}

			return string.Empty;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}