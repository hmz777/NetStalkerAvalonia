using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.Views;
using Splat;
using System;

namespace NetStalkerAvalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                desktop.ShutdownRequested += Desktop_ShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            var seriLogger = Locator.Current.GetService<ILogger>();

            if (seriLogger != null)
            {
                var disposable = seriLogger as IDisposable;
                disposable?.Dispose();
            }
        }
    }
}
