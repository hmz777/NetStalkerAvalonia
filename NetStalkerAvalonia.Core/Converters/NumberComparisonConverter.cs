using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace NetStalkerAvalonia.Core.Converters
{
	public class NumberComparisonConverter : IValueConverter
	{
		public readonly static NumberComparisonConverter Instance = new();

		private string[] operators = new string[] { ">", "<", "=", ">=", "<=" };

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is null || parameter is null || double.TryParse(value.ToString(), out double sourceNumber) == false)
			{
				return false;
			}

			try
			{
				if (parameter.ToString()!.Split(' ') is [var op, var number] &&
					operators.Contains(op) &&
					double.TryParse(number, out double numberToCompare))
				{
					switch (op)
					{
						case ">":
							return sourceNumber > numberToCompare;
						case ">=":
							return sourceNumber >= numberToCompare;
						case "<":
							return sourceNumber < numberToCompare;
						case "<=":
							return sourceNumber <= numberToCompare;
						case "=":
							return sourceNumber == numberToCompare;
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
