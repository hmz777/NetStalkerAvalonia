using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace NetStalkerAvalonia.Components
{
    public partial class TitleBar : UserControl
    {
        private Window? _window;

        public TitleBar()
        {
            InitializeComponent();

            this.Initialized += (sender, args) => { AttachDraggingBehaviour(); };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MaximizeWindow(Window window)
        {
            if (window.WindowState == WindowState.Normal)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeWindow(Window window)
        {
            window.WindowState = WindowState.Minimized;
        }

        private void CloseWindow(Window window)
        {
            window.Close();
        }

        private void AttachDraggingBehaviour()
        {
            this.PointerPressed += TitleBar_PointerPressed;
        }

        private void TitleBar_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var mouseDevice = e.GetCurrentPoint(this);

            if (mouseDevice.Properties.IsLeftButtonPressed)
            {
                if (_window == null)
                {
                    _window = (Application.Current?.ApplicationLifetime
                                 as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

                    return;
                }

                _window.BeginMoveDrag(e);
            }
        }
    }
}