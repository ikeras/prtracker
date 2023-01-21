using Microsoft.Extensions.DependencyInjection;

namespace PRTracker.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public static TrackerTrayIconViewModel TrackerTrayIcon => App.Current.Services.GetService<TrackerTrayIconViewModel>();

        public static void Cleanup()
        {
        }
    }
}