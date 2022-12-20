using NetStalkerAvalonia.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Services
{
	public class AppSettings
	{
		public bool NotificationSuppression { get; set; }
		public bool SpoofProtection { get; set; }

		public AppSettings()
		{
			NotificationSuppression = Convert.ToBoolean(ConfigurationManager.AppSettings[nameof(ConfigKeys.NotificationSuppression)]);
			SpoofProtection = Convert.ToBoolean(ConfigurationManager.AppSettings[nameof(ConfigKeys.SpoofProtection)]);			
		}

		public void SaveChanges()
		{
			ConfigurationManager.AppSettings[nameof(ConfigKeys.NotificationSuppression)] = NotificationSuppression.ToString();
			ConfigurationManager.AppSettings[nameof(ConfigKeys.SpoofProtection)] = SpoofProtection.ToString();
		}
	}
}