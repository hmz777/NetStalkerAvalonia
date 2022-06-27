using System.Threading.Tasks;
using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IDeviceNameResolver
    {
        Task GetDeviceNameAsync(Device device);
    }
}