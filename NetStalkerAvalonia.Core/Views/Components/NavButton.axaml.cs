using Avalonia;
using Avalonia.Controls.Primitives;
using System.Windows.Input;

namespace NetStalkerAvalonia.Core.Views.Components
{
	public class NavButton : TemplatedControl
	{
		public static readonly StyledProperty<string> TextProperty =
		AvaloniaProperty.Register<NavButton, string>(nameof(Text), defaultValue: "Button Text");

		public string Text
		{
			get { return GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly StyledProperty<string> IconProperty =
				AvaloniaProperty.Register<NavButton, string>(nameof(Icon), defaultValue: "Assets/netstalker-logo.ico");

		public string Icon
		{
			get { return GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static readonly StyledProperty<ICommand> CommandProperty =
				AvaloniaProperty.Register<NavButton, ICommand>(nameof(Command));

		public ICommand Command
		{
			get { return GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public static readonly StyledProperty<object> CommandParameterProperty =
				AvaloniaProperty.Register<NavButton, object>(nameof(CommandParameter));

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}
	}
}