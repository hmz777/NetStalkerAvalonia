using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System.Windows.Input;

namespace NetStalkerAvalonia.Components
{
	public class GenericIconButton : TemplatedControl
	{
		public static readonly StyledProperty<string> TextProperty =
		AvaloniaProperty.Register<LinkButton, string>(nameof(Text), defaultValue: "Button Text");

		public string Text
		{
			get { return GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly StyledProperty<string> IconProperty =
				AvaloniaProperty.Register<LinkButton, string>(nameof(Icon), defaultValue: "/Assets/avalonia-logo.ico");

		public string Icon
		{
			get { return GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static readonly StyledProperty<ICommand> CommandProperty =
				AvaloniaProperty.Register<LinkButton, ICommand>(nameof(Command));

		public ICommand Command
		{
			get { return GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public static readonly StyledProperty<object?> CommandParameterProperty =
				AvaloniaProperty.Register<LinkButton, object?>(nameof(CommandParameter));

		public object? CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}
	}
}
