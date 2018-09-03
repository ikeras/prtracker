namespace PRTrackerUI.ViewServices
{
    internal interface INotificationService
    {
        void ShowNotification(string title, string message, NotificationType notificationType);
    }
}
