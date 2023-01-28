using Avalonia;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.BlockingRedirection;
using NetStalkerAvalonia.Services.Implementations.DeviceNameResolving;
using NetStalkerAvalonia.Services.Implementations.DeviceTypeIdentification;
using NetStalkerAvalonia.Services.Implementations.Notifications;
using NetStalkerAvalonia.Services.Implementations.Packets;
using Serilog;
using Splat;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Reflection;
using NetStalkerAvalonia.Configuration;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services.Implementations.DeviceScanning;
using ReactiveUI;
using NetStalkerAvalonia.Services.Implementations.AppLocking;
using Avalonia.Controls.ApplicationLifetimes;
using NetStalkerAvalonia.Services.Implementations.RulesService;
using AutoMapper;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using NetStalkerAvalonia.Rules;
using NetStalkerAvalonia.Rules.Implementations;

namespace NetStalkerAvalonia
{
	internal class Program
	{
		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
				BuildAvaloniaApp()
					.StartWithClassicDesktopLifetime(args);
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), nameof(Main), e.Message);
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		private static AppBuilder BuildAvaloniaApp()
		{
			// Router uses Splat.Locator to resolve views for
			// view models, so we need to register our views.
			Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

			// Read app config
			ReadConfiguration();

			// Configure logging
			ConfigureAndRegisterLogging();

			// Register required app services
			RegisterRequiredServices();

			// Register optional services
			RegisterOptionalServices();

			return AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.LogToTrace()
				.UseReactiveUI();
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
			Locator.CurrentMutable.RegisterLazySingleton(() =>
					new DeviceScanner(),
				typeof(IDeviceScanner));

			Locator.CurrentMutable.RegisterLazySingleton(() =>
					new BlockerRedirector(),
				typeof(IBlockerRedirector));

			Locator.CurrentMutable.RegisterLazySingleton(() =>
					new DeviceNameResolver(),
				typeof(IDeviceNameResolver));

			Locator.CurrentMutable.RegisterLazySingleton(() =>
					new NotificationManager(),
				typeof(INotificationManager));

			Locator.CurrentMutable.RegisterLazySingleton(() =>
					new AppLockManager(),
				typeof(IAppLockService));

			Locator.CurrentMutable.RegisterLazySingleton(() =>
					new RuleService(),
				typeof(IRuleService));

			ConfigureAndRegisterAutoMapper();
		}

		private static void RegisterOptionalServices()
		{
			var apiToken = Config.AppSettings?.VendorApiTokenSetting;

			if (string.IsNullOrWhiteSpace(apiToken) == false)
			{
				Locator.CurrentMutable.RegisterLazySingleton(() =>
				{
					var client = new HttpClient();
					client.DefaultRequestHeaders.Add("Authorization",
						"Bearer " + apiToken);

					return client;
				}, contract: nameof(ContractKeys.MacLookupClient));

				Locator.CurrentMutable.RegisterLazySingleton(() =>
						new DeviceTypeIdentifier(),
					typeof(IDeviceTypeIdentifier));

				OptionalFeatures.AvailableFeatures.Add(typeof(IDeviceTypeIdentifier));
			}
		}

		private static void ConfigureAndRegisterAutoMapper()
		{
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RuleBase, AddUpdateRuleModel>().ReverseMap();

				cfg.CreateMap<BlockRule, AddUpdateRuleModel>().ReverseMap();

				cfg.CreateMap<RedirectRule, AddUpdateRuleModel>().ReverseMap();

				cfg.CreateMap<LimitRule, AddUpdateRuleModel>().ReverseMap();

				cfg.CreateMap<BlockRule, RuleBase>().ReverseMap();

				cfg.CreateMap<RedirectRule, RuleBase>().ReverseMap();

				cfg.CreateMap<LimitRule, RuleBase>().ReverseMap();

				cfg.CreateMap<LimitRule, LimitRule>().ReverseMap();
			});

			Mapper mapper = new(mapperConfig);

			Locator.CurrentMutable.RegisterConstant(mapper, typeof(IMapper));
		}

		private static void ReadConfiguration()
		{
			Config.AppSettings = new AppSettings();
			Config.AppSettings.ReadConfiguration();
		}
	}
}