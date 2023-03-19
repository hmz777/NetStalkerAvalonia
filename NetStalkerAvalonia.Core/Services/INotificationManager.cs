using NetStalkerAvalonia.Core.Services.Implementations.Notifications;

namespace NetStalkerAvalonia.Core.Services
{
    public interface INotificationManager
    {
        void SendNotification();
        void ClearNotifications();
        void DestroyService();
    }
}
