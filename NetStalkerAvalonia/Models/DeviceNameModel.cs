using System.Net.NetworkInformation;

namespace NetStalkerAvalonia.Models;

public class DeviceNameModel
{
    public PhysicalAddress Mac { get; set; }
    public string? Name { get; set; }

    public DeviceNameModel(PhysicalAddress mac, string? name)
    {
        Mac = mac;
        Name = name;
    }
}