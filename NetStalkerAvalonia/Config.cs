using NetStalkerAvalonia.Services;
using System.Text.Json;

namespace NetStalkerAvalonia
{
	public class Config
	{
		public static AppSettings? AppSettings { get; set; }
		public static JsonSerializerOptions JsonSerializerOptions = new();
	}
}
