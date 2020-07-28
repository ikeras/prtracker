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
            BalloonIcon balloonIcon;

            switch (notificationType)
            {
                case NotificationType.None:
                    balloonIcon = BalloonIcon.None;
                    break;
                case NotificationType.Info:
                    balloonIcon = BalloonIcon.Info;
                    break;
                case NotificationType.Warning:
                    balloonIcon = BalloonIcon.Warning;
                    break;
                case NotificationType.Error:
                    balloonIcon = BalloonIcon.Error;
                    break;
                default:
                    balloonIcon = BalloonIcon.None;
                    break;
            }

            this.taskBarIcon.ShowBalloonTip(title, message, balloonIcon);
        }
    }
}
