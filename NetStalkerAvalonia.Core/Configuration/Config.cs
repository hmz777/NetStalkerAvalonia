using NetStalkerAvalonia.Core.Services;
using System.Text.Json;

namespace NetStalkerAvalonia.Core.Configuration
{
    public class Config
    {
        public static AppSettings AppSettings { get; set; } = new AppSettings();
        public static JsonSerializerOptions JsonSerializerOptions = new();

        public static void ReadConfiguration()
        {
			AppSettings.ReadConfiguration();
		}
    }
}