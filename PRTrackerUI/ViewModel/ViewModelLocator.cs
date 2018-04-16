using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using PRServicesClient.Services;

namespace PRTrackerUI.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IConnectionService, PullRequestServices>();
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel Main
        {
            get => ServiceLocator.Current.GetInstance<MainViewModel>();
        }

        public static void Cleanup()
        {
        }
    }
}