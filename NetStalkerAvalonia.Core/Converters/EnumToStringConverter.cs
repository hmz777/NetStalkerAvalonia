using Avalonia.Data.Converters;
using NetStalkerAvalonia.Core.Rules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Converters
{
	public class EnumToStringConverter : IValueConverter
	{
		public readonly static EnumToStringConverter Instance = new();
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is null)
			{
				return null;
			}

			return value.ToString();
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is null)
			{
				return null;
			}

			switch (parameter)
			{
				case "Action":
					if (Enum.IsDefined(typeof(RuleAction), value) == false)
					{
						return null;
					}

					return Enum.Parse(typeof(RuleAction), value.ToString()!);
				case "Source":
					if (Enum.IsDefined(typeof(RuleSourceValue), value) == false)
					{
						return null;
					}

					return Enum.Parse(typeof(RuleSourceValue), value.ToString()!);
				default:
					break;
			}

			return null;
		}
	}
}
