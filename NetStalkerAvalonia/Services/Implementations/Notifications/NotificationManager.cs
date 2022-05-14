using System;

namespace NetStalkerAvalonia.Services.Implementations.Notifications
{
    public class NotificationManager : INotificationManager
    {
        private readonly NotificationOptions notificationOptions;

        public NotificationManager(NotificationOptions notificationOptions)
        {
            this.notificationOptions = notificationOptions;
        }

        public void ClearNotifications()
        {
            throw new NotImplementedException();
        }

        public void DestroyService()
        {
            throw new NotImplementedException();
        }

        public void SendNotification(NotificationOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
