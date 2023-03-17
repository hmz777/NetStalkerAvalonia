using NetStalkerAvalonia.Services.Implementations.Notifications;

namespace NetStalkerAvalonia.Services
{
    public interface INotificationManager
    {
        void SendNotification();
        void ClearNotifications();
        void DestroyService();
    }
}
