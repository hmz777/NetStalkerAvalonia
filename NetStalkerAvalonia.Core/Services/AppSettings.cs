using NetStalkerAvalonia.Core.Configuration;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Services
{
	public class AppSettings : ReactiveObject
	{
		public bool NotificationSuppressionSetting { get; set; }
		public bool SpoofProtectionSetting { get; set; }
		public string? VendorApiTokenSetting { get; set; }
		public bool MinimizeToTraySetting { get; set; }
		public bool MinimizeToTaskbarSetting { get; set; } = true;

		public void ReadConfiguration()
		{
			var settings = ConfigurationManager.AppSettings;
			var properties = typeof(AppSettings).GetProperties();

			foreach (var item in settings.AllKeys)
			{
				var property = properties.FirstOrDefault(p => p.Name == item && p.Name.EndsWith("Setting"));
				var type = property?.PropertyType;
				var value = settings[item];

				property?.SetValue(this, Convert.ChangeType(value, type!));
			}
		}

		public void SaveChanges()
		{
			var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var settings = configFile.AppSettings.Settings;

			foreach (var property in typeof(AppSettings).GetProperties().Where(p => p.Name.EndsWith("Setting")))
			{
				var key = property.Name;
				var value = property.GetValue(this)?.ToString();

				if (settings[key] == null)
				{
					settings.Add(key, value);
				}
				else
				{
					settings[key].Value = value;
				}
			}

			configFile.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
		}
	}
}