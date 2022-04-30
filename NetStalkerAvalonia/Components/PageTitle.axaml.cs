using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NetStalkerAvalonia.Components
{
    public partial class PageTitle : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly AttachedProperty<string> TextProperty =
                      AttachedProperty<string>.RegisterAttached<ImageButton, string>(nameof(Text), typeof(ImageButton));

        public PageTitle()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
