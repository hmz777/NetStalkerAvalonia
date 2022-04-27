using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Windows.Input;

namespace NetStalkerAvalonia.Components
{
    public partial class ImageButton : UserControl
    {
        public ImageButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly AttachedProperty<string> TextProperty =
                      AttachedProperty<string>.RegisterAttached<ImageButton, string>(nameof(Text), typeof(ImageButton));

        public string IconSource
        {
            get { return (string)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        public static readonly AttachedProperty<string> IconSourceProperty =
                      AttachedProperty<string>.RegisterAttached<ImageButton, string>(nameof(IconSource), typeof(ImageButton));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly AttachedProperty<ICommand> CommandProperty =
                      AttachedProperty<ICommand>.RegisterAttached<ImageButton, ICommand>(nameof(Command), typeof(ImageButton));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly AttachedProperty<object> CommandParameterProperty =
                      AttachedProperty<object>.RegisterAttached<ImageButton, object>(nameof(CommandParameter), typeof(ImageButton));
    }
}