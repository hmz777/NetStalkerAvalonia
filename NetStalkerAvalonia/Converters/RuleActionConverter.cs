using Avalonia.Data.Converters;
using NetStalkerAvalonia.Rules;
using System;
using System.Globalization;

namespace NetStalkerAvalonia.Converters
{
	public class RuleActionConverter : IValueConverter
	{
		public static readonly RuleActionConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (Enum.IsDefined(typeof(RuleAction), value!) &&
				parameter is string actionType &&
				targetType.IsAssignableTo(typeof(bool)))
			{
				var action = (RuleAction)value!;

				switch (actionType)
				{
					case nameof(RuleAction.Block):
						return action == RuleAction.Block;
					case nameof(RuleAction.Redirect):
						return action == RuleAction.Redirect;
					case nameof(RuleAction.Limit):
						return action == RuleAction.Limit;
					default:
						break;
				}
			}

			return false;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
