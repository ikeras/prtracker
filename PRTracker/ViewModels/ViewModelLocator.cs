using GalaSoft.MvvmLight.Ioc;
using PRServices.Services;

namespace PRTracker.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<IConnectionService, ConnectionService>();
            SimpleIoc.Default.Register<TrackerTrayIconViewModel>();
        }

        public TrackerTrayIconViewModel TrackerTrayIcon
        {
            get => SimpleIoc.Default.GetInstance<TrackerTrayIconViewModel>();
        }

        public static void Cleanup()
        {
        }
    }
}