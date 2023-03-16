using AutoMapper;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.AppLocking;
using NetStalkerAvalonia.Services.Implementations.BlockingRedirection;
using NetStalkerAvalonia.Services.Implementations.DeviceNameResolving;
using NetStalkerAvalonia.Services.Implementations.DeviceScanning;
using NetStalkerAvalonia.Services.Implementations.DeviceTypeIdentification;
using NetStalkerAvalonia.Services.Implementations.Notifications;
using NetStalkerAvalonia.Services.Implementations.PcapDeviceManagement;
using NetStalkerAvalonia.Services.Implementations.RulesService;
using NetStalkerAvalonia.Services.Implementations.ViewRouting;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using Serilog;
using Splat;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Reflection;

namespace NetStalkerAvalonia
{
	public class DependencyInjection
	{
		public static void RegisterAppDependencies()
		{
			// Configure logging
			ConfigureAndRegisterLogging();

			// Register required app services
			RegisterRequiredServices();

			RegisterViewModels();

			// Register optional services
			RegisterOptionalServices();
		}

		private static void ConfigureAndRegisterLogging()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.File(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "logs/log.txt"),
					rollingInterval: RollingInterval.Day)
				.CreateLogger();
		}

		private static void RegisterRequiredServices()
		{
			SplatRegistrations.SetupIOC(Locator.GetLocator());

			SplatRegistrations.RegisterLazySingleton<IFileSystem, FileSystem>();
			SplatRegistrations.RegisterLazySingleton<IDeviceTypeIdentifier, DeviceTypeIdentifier>();
			SplatRegistrations.RegisterLazySingleton<IDeviceNameResolver, DeviceNameResolver>();
			SplatRegistrations.RegisterLazySingleton<IPcapDeviceManager, PcapDeviceManager>();
			SplatRegistrations.RegisterLazySingleton<IDeviceScanner, DeviceScanner>();
			SplatRegistrations.RegisterLazySingleton<IBlockerRedirector, BlockerRedirector>();
			SplatRegistrations.RegisterLazySingleton<INotificationManager, NotificationManager>();
			SplatRegistrations.RegisterLazySingleton<IAppLockService, AppLockManager>();
			SplatRegistrations.RegisterLazySingleton<IRuleService, RuleService>();

			ConfigureAndRegisterAutoMapper();
		}

		private static void RegisterViewModels()
		{
			SplatRegistrations.RegisterLazySingleton<IRouter, ViewRouter>();
			SplatRegistrations.RegisterLazySingleton<MainViewModel>();
			SplatRegistrations.RegisterLazySingleton<SnifferViewModel>();
			SplatRegistrations.RegisterLazySingleton<OptionsViewModel>();
			SplatRegistrations.RegisterLazySingleton<RuleBuilderViewModel>();
			SplatRegistrations.RegisterLazySingleton<HelpViewModel>();
			SplatRegistrations.RegisterLazySingleton<AboutViewModel>();
			SplatRegistrations.RegisterLazySingleton<AppLogViewModel>();
			SplatRegistrations.RegisterLazySingleton<AdapterSelectViewModel>();
			SplatRegistrations.RegisterLazySingleton<PasswordViewModel>();
		}

		private static void RegisterOptionalServices()
		{
			var apiToken = Config.AppSettings.VendorApiTokenSetting;

			if (string.IsNullOrWhiteSpace(apiToken) == false)
			{
				SplatRegistrations.RegisterConstant(() =>
				{
					var client = new HttpClient();
					client.DefaultRequestHeaders.Add("Authorization",
						"Bearer " + apiToken);

					return client;
				});
			}
		}

		private static void ConfigureAndRegisterAutoMapper()
		{
			var mapper = Tools.BuildAutoMapper();

			SplatRegistrations.RegisterConstant<IMapper>(mapper);
		}
	}
}