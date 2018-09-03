using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using Hardcodet.Wpf.TaskbarNotification;
using PRTrackerUI.ViewServices;

namespace PRTrackerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotificationService
    {
        public MainWindow()
        {
            this.InitializeComponent();
            SimpleIoc.Default.Register<INotificationService>(() => this);
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

            this.prTrackerTaskBarIcon.ShowBalloonTip(title, message, balloonIcon);
        }
    }
}
