using NetStalkerAvalonia.Services.Implementations.Notifications;

namespace NetStalkerAvalonia.Services
{
    public interface INotificationManager
    {
        void SendNotification(NotificationOptions options);
        void ClearNotifications();
        void DestroyService();
    }
}
