using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
    public interface IBlockerRedirector
    {
        void Block(Device device);
        void UnBlock(Device device);
        void Redirect(Device device);
        void UnRedirect(Device device);
    }
}