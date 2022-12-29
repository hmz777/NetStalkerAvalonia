using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Devices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;
using Serilog;
using Serilog.Core;
using SharpPcap;
using SharpPcap.LibPcap;
using Splat;
using System.Diagnostics;

namespace NetStalkerAvalonia.Helpers;

public class Tools
{
	public static List<ViewModelBase> ViewModels = new();

	public static void InitViewModels()
	{
		var mainViewModel = new MainWindowViewModel();

		ViewModels = new List<ViewModelBase>
		{
			mainViewModel,
			new SnifferViewModel(mainViewModel),
			new OptionsViewModel(mainViewModel),
			new RuleBuilderViewModel(mainViewModel),
			new HelpViewModel(mainViewModel),
			new AboutViewModel(mainViewModel)
		};
	}

	public static T ResolveIfNull<T>(T dependency, string contract = null)
	{
		if (dependency != null)
			return dependency;

		dependency = Locator.Current.GetService<T>(contract)!;

		if (dependency == null &&
			OptionalFeatures.AvailableFeatures.Contains(typeof(T)) == false)
		{
			// Only throw on non-optional features
			throw new Exception(string.Format("The dependency locator returned null of type {0}.", typeof(T)));
		}

		return dependency!;
	}

	public static string GetRootIp(IPAddress ipaddress, NetworkClass networkClass)
	{
		ArgumentNullException.ThrowIfNull(ipaddress);

		var ipaddressstring = ipaddress.ToString();

		switch (networkClass)
		{
			case NetworkClass.A:
				return ipaddressstring
					.Substring(0,
						ipaddressstring
							.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1);
			case NetworkClass.B:
				return ipaddressstring
					.Substring(0,
						ipaddressstring
							.IndexOf(".", 4, StringComparison.InvariantCultureIgnoreCase) + 1);
			case NetworkClass.C:
				return ipaddressstring
					.Substring(0,
						ipaddressstring
							.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1);
			default:
				throw new ArgumentOutOfRangeException(nameof(networkClass), networkClass, null);
		}
	}

	public static void ResolveGateway()
	{
		var adapterName = (from devicex in LibPcapLiveDeviceList.Instance
						   where devicex.Interface.FriendlyName == HostInfo.NetworkAdapterName
						   select devicex).ToList()[0].Name;

		using (var device = LibPcapLiveDeviceList.New()[adapterName])
		{
			var gatewayArp = new ARP(device);
			var gatewayMac = gatewayArp.Resolve(HostInfo.GatewayIp);

			if (gatewayMac == null)
			{
				throw new Exception("Couldn't resolve gateway mac address.");
			}

			HostInfo.GatewayMac = gatewayMac;
		}
	}

	public static void HandleError(Interaction<StatusMessage, Unit> interaction, StatusMessage statusMessage)
	{
		ArgumentNullException.ThrowIfNull(interaction, nameof(interaction));

		Log.Error("Exception triggered with message:{Message}", statusMessage.Message);

		interaction.Handle(statusMessage);
	}

	public static void ExitApp()
	{
		var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
		app?.Shutdown();
	}

	public static void OpenLink(string link)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = link,
			UseShellExecute = true
		});
	}

	public static byte[]? StringEncrypt(string stringToEncrypt, byte[] key = null, byte[] iv = null)
	{
		byte[]? encryptedText = null;
		try
		{
			using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToEncrypt)))
			using (var outputStream = new MemoryStream())
			using (var aes = Aes.Create())
			{
				aes.Padding = System.Security.Cryptography.PaddingMode.ISO10126;
				aes.KeySize = 256;

				using (var cryptoStream = new CryptoStream(outputStream,
						   aes.CreateEncryptor(key, iv),
						   CryptoStreamMode.Write))
				{
					var buffer = new byte[1024];
					var read = inputStream.Read(buffer, 0, buffer.Length);
					while (read > 0)
					{
						cryptoStream.Write(buffer, 0, read);
						read = inputStream.Read(buffer, 0, buffer.Length);
					}

					cryptoStream.FlushFinalBlock();
					encryptedText = outputStream.ToArray();
				}
			}
		}
		catch (Exception e)
		{
			Log.Error(LogMessageTemplates.ExceptionTemplate,
				e.GetType(), nameof(StringEncrypt), e.Message);
		}

		return encryptedText;
	}

	public static byte[]? StringDecrypt(byte[] bytesToDecrypt, byte[]? key = null, byte[]? iv = null)
	{
		byte[]? finalBytes = null;

		try
		{
			using (var input = new MemoryStream(bytesToDecrypt))
			using (var output = new MemoryStream())
			using (var aes = Aes.Create())
			{
				aes.Padding = PaddingMode.ISO10126;
				aes.KeySize = 256;

				using (var cryptoStream =
					   new CryptoStream(input, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
				{
					var buffer = new byte[1024];
					var read = cryptoStream.Read(buffer, 0, buffer.Length);
					while (read > 0)
					{
						output.Write(buffer, 0, read);
						read = cryptoStream.Read(buffer, 0, buffer.Length);
					}

					cryptoStream.Flush();
					finalBytes = output.ToArray();
				}
			}
		}
		catch (Exception e)
		{
			Log.Error(LogMessageTemplates.ExceptionTemplate,
				e.GetType(), nameof(StringEncrypt), e.Message);
		}

		return finalBytes;
	}
}