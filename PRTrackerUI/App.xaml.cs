using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using Hardcodet.Wpf.TaskbarNotification;
using PRTrackerUI.ViewServices;

namespace PRTrackerUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon trackerTaskBarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.trackerTaskBarIcon = (TaskbarIcon)this.FindResource("TrackerTaskBarIcon");
            SimpleIoc.Default.Register<INotificationService>(() => new NotificationService(this.trackerTaskBarIcon));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.trackerTaskBarIcon.Dispose();

            base.OnExit(e);
        }
    }
}
