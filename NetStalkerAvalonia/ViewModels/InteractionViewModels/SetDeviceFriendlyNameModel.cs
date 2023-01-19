using System.Net.NetworkInformation;

namespace NetStalkerAvalonia.ViewModels.InteractionViewModels;

public class SetDeviceFriendlyNameModel
{
    public string? Name { get; set; }
    public PhysicalAddress? Mac { get; set; }

    public SetDeviceFriendlyNameModel(string? name, PhysicalAddress? mac)
    {
        Name = name;
        Mac = mac;
    }
}