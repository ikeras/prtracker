using H.NotifyIcon;
using H.NotifyIcon.Core;

namespace PRTracker.ViewServices
{
    public class NotificationService : INotificationService
    {
        private readonly TaskbarIcon taskBarIcon;

        public NotificationService(TaskbarIcon taskBarIcon)
        {
            this.taskBarIcon = taskBarIcon;
        }

        public void ShowNotification(string title, string message, NotificationType notificationType)
        {
            NotificationIcon notificationIcon = notificationType switch
            {
                NotificationType.None => NotificationIcon.None,
                NotificationType.Info => NotificationIcon.Info,
                NotificationType.Warning => NotificationIcon.Warning,
                NotificationType.Error => NotificationIcon.Error,
                _ => NotificationIcon.None,
            };

            if (this.taskBarIcon.IsCreated)
            {
                this.taskBarIcon.ShowNotification(title, message, notificationIcon);
            }
        }
    }
}
