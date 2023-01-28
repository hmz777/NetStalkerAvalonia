using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Windows.Input;

namespace NetStalkerAvalonia.Components
{
	public partial class NavButton : UserControl
	{
		public NavButton()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public string TextContent
		{
			get { return (string)GetValue(TextContentProperty); }
			set { SetValue(TextContentProperty, value); }
		}

		public static readonly AttachedProperty<string> TextContentProperty =
					  AttachedProperty<string>.RegisterAttached<NavButton, string>(nameof(TextContent), typeof(NavButton));

		public string IconSource
		{
			get { return (string)GetValue(IconSourceProperty); }
			set { SetValue(IconSourceProperty, value); }
		}

		public static readonly AttachedProperty<string> IconSourceProperty =
					  AttachedProperty<string>.RegisterAttached<NavButton, string>(nameof(IconSource), typeof(NavButton));

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public static readonly AttachedProperty<ICommand> CommandProperty =
					  AttachedProperty<ICommand>.RegisterAttached<NavButton, ICommand>(nameof(Command), typeof(NavButton));

		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		public static readonly AttachedProperty<object> CommandParameterProperty =
					  AttachedProperty<object>.RegisterAttached<NavButton, object>(nameof(CommandParameter), typeof(NavButton));
	}	
}