using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace NetStalkerAvalonia.Core.Converters
{
	public class StringEqualsConverter : IValueConverter
	{
		private string[] operators = new string[] { "!=", "=" };
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null || parameter == null) return false;

			var valueStr = value as string;

			try
			{
				if (parameter.ToString()!.Split('|') is [var op, var str] &&
					operators.Contains(op))
				{
					switch (op)
					{
						case "!=":
							return valueStr != str;
						case "=":
							return valueStr == str;
						default:
							break;
					};
				}
			}
			catch { }

			return false;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
