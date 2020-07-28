using Hardcodet.Wpf.TaskbarNotification;

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
            BalloonIcon balloonIcon = notificationType switch
            {
                NotificationType.None => BalloonIcon.None,
                NotificationType.Info => BalloonIcon.Info,
                NotificationType.Warning => BalloonIcon.Warning,
                NotificationType.Error => BalloonIcon.Error,
                _ => BalloonIcon.None,
            };

            this.taskBarIcon.ShowBalloonTip(title, message, balloonIcon);
        }
    }
}
