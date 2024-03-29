﻿using AutoMapper;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.Services.Implementations;
using NetStalkerAvalonia.Core.Services.Implementations.BlockingRedirection;
using NetStalkerAvalonia.Core.Services.Implementations.DeviceNameResolving;
using NetStalkerAvalonia.Core.Services.Implementations.DeviceScanning;
using NetStalkerAvalonia.Core.Services.Implementations.DeviceTypeIdentification;
using NetStalkerAvalonia.Core.Services.Implementations.ErrorHandling;
using NetStalkerAvalonia.Core.Services.Implementations.MessageBus;
using NetStalkerAvalonia.Core.Services.Implementations.Notifications;
using NetStalkerAvalonia.Core.Services.Implementations.PcapDeviceManagement;
using NetStalkerAvalonia.Core.Services.Implementations.RulesService;
using NetStalkerAvalonia.Core.Services.Implementations.StatusMessages;
using NetStalkerAvalonia.Core.Services.Implementations.ViewRouting;
using NetStalkerAvalonia.Core.ViewModels;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;
using ReactiveUI;
using Serilog;
using Splat;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Reflection;

namespace NetStalkerAvalonia.Core.Helpers
{
	public class DependencyInjectionHelpers
	{
		public static T ResolveIfNull<T>(T dependency, string contract = null!)
		{
			if (dependency != null)
				return dependency;

			dependency = Locator.Current.GetService<T>(contract)!;

			if (dependency == null)
			{
				// Only throw on non-optional features
				throw new Exception(string.Format("The dependency locator returned null of type {0}.", typeof(T)));
			}

			return dependency!;
		}

		public static void RegisterAppDependencies()
		{
			// Configure logging
			ConfigureAndRegisterLogging();

			// Register required app services
			RegisterRequiredServices();

			// Register optional services
			RegisterOptionalServices();

			RegisterViewModels();

			RegisterViewsForViewModels();
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

			SplatRegistrations.RegisterLazySingleton<IRouter, ViewRouter>();
			SplatRegistrations.RegisterLazySingleton<IFileSystem, FileSystem>();
			SplatRegistrations.RegisterLazySingleton<IDeviceTypeIdentifier, DeviceTypeIdentifier>();
			SplatRegistrations.RegisterLazySingleton<IDeviceNameResolver, DeviceNameResolver>();
			SplatRegistrations.RegisterLazySingleton<IPcapDeviceManager, PcapDeviceManager>();
			SplatRegistrations.RegisterLazySingleton<IDeviceScanner, DeviceScanner>();
			SplatRegistrations.RegisterLazySingleton<IBlockerRedirector, BlockerRedirector>();
			SplatRegistrations.RegisterLazySingleton<INotificationManager, NotificationManager>();
			SplatRegistrations.RegisterLazySingleton<IRuleService, RuleService>();
			SplatRegistrations.RegisterLazySingleton<IErrorHandler, ErrorHandler>();
			SplatRegistrations.RegisterLazySingleton<IMessageBusService, MessageBusService>();
			SplatRegistrations.RegisterLazySingleton<IStatusMessageService, StatusMessageService>();

			ConfigureAndRegisterAutoMapper();
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
			var mapper = AutoMapperHelpers.BuildAutoMapper();

			SplatRegistrations.RegisterConstant<IMapper>(mapper);
		}

		private static void RegisterViewModels()
		{
			SplatRegistrations.RegisterLazySingleton<SnifferViewModel>();
			SplatRegistrations.RegisterLazySingleton<OptionsViewModel>();
			SplatRegistrations.RegisterLazySingleton<RuleBuilderViewModel>();
			SplatRegistrations.RegisterLazySingleton<HelpViewModel>();
			SplatRegistrations.RegisterLazySingleton<AboutViewModel>();
			SplatRegistrations.RegisterLazySingleton<AppLogViewModel>();
			SplatRegistrations.RegisterLazySingleton<AdapterSelectViewModel>();
			SplatRegistrations.RegisterLazySingleton<PasswordViewModel>();
			SplatRegistrations.Register<HomeViewModel>();
			SplatRegistrations.Register<MainViewModel>();
		}

		private static void RegisterViewsForViewModels()
		{
			Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
		}
	}
}