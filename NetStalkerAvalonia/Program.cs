using Avalonia;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.BandwidthControl;
using NetStalkerAvalonia.Services.Implementations.BlockingRedirection;
using NetStalkerAvalonia.Services.Implementations.DeviceNameResolving;
using NetStalkerAvalonia.Services.Implementations.DeviceTypeIdentification;
using NetStalkerAvalonia.Services.Implementations.Notifications;
using NetStalkerAvalonia.Services.Implementations.Packets;
using ReactiveUI;
using Serilog;
using Splat;
using System;
using System.Configuration;
using System.Reflection;
using ILogger = Serilog.ILogger;

namespace NetStalkerAvalonia
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            // Router uses Splat.Locator to resolve views for
            // view models, so we need to register our views.
            //
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            // Configure logging
            ConfigureLogging();

            // Register app services
            RegisterRequiredServices();

            return AppBuilder.Configure<App>()
                 .UsePlatformDetect()
                 .LogToTrace()
                 .UseReactiveUI();
        }

        private static void ConfigureLogging()
        {
            var log = new LoggerConfiguration()
                                .WriteTo.Console()
                                .WriteTo.File(Assembly.GetExecutingAssembly().Location, rollingInterval: RollingInterval.Day)
                                .CreateLogger();

            Locator.CurrentMutable.Register<ILogger>(() => log);
        }

        private static void RegisterRequiredServices()
        {
            Locator.CurrentMutable.Register<IBandwidthController>(() => new BandwidthController());
            Locator.CurrentMutable.Register<IBlockerRedirector>(() => new BlockerRedirector());
            Locator.CurrentMutable.Register<IDeviceNameResolver>(() => new DeviceNameResolver());
            Locator.CurrentMutable.Register<IDeviceTypeIdentifier>(() => new DeviceTypeIdentifier());

            var notificationOptions = ConfigurationManager.GetSection("Notifications") as NotificationOptions
                                      ?? new NotificationOptions();

            Locator.CurrentMutable.Register<INotificationManager>(() => new NotificationManager(notificationOptions));
            Locator.CurrentMutable.Register<IPacketManager>(() => new PacketManager());
        }
    }
}