using System;
using System.Windows;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using PRServices.Services;
using PRTracker.ViewModels;
using PRTracker.ViewServices;

namespace PRTracker
{
    /// <summary>
    /// Interaction logic for App.
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon trackerTaskBarIcon;

        public static new App Current => (App)Application.Current;

        public IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceCollection services = new();
            services.AddSingleton<INotificationService>((sp) => new NotificationService(this.trackerTaskBarIcon));
            services.AddSingleton<IConnectionService, ConnectionService>();
            services.AddSingleton<TrackerTrayIconViewModel>();

            this.Services = services.BuildServiceProvider();

            this.trackerTaskBarIcon = (TaskbarIcon)this.FindResource("TrackerTaskBarIcon");
            this.trackerTaskBarIcon.ForceCreate();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.trackerTaskBarIcon.Dispose();

            base.OnExit(e);
        }
    }
}
