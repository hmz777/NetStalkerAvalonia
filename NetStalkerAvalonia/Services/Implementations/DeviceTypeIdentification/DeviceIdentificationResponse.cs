using System.Text.Json.Serialization;

namespace NetStalkerAvalonia.Services.Implementations.DeviceTypeIdentification;

public class DeviceIdentificationResponse
{
	[JsonPropertyName("assignment")]
	public string? Assignment { get; set; }

	[JsonPropertyName("organization_address")]
	public string? OrganizationAddress { get; set; }

	[JsonPropertyName("organization_name")]
	public string? OrganizationName { get; set; }

	[JsonPropertyName("registry")]
	public string? Registry { get; set; }
}