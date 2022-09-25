using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.Views;
using Splat;
using System;
using System.Linq;
using Windows.Globalization.PhoneNumberFormatting;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;
using Serilog;

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
            try
            {
                Tools.InitViewModels();

                var mainViewModel = Tools.ViewModels.First() as MainWindowViewModel;

                if (mainViewModel == null)
                {
                    throw new Exception("Error initializing view models!");
                }

                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = mainViewModel
                    };

                    desktop.ShutdownRequested += async (sender, args) =>
                    {
                        // Save device friendly names before exiting
                        var deviceNameResolver = Tools.ResolveIfNull<IDeviceNameResolver>(null!);
                        await deviceNameResolver.SaveDeviceNamesAsync(mainViewModel.GetUiDeviceCollection());
                    };
                }

                base.OnFrameworkInitializationCompleted();
            }
            catch (Exception e)
            {
                Log.Error(LogMessageTemplates.ExceptionTemplate,
                    e.GetType(), this.GetType(), e.Message);
                throw;
            }
        }
    }
}