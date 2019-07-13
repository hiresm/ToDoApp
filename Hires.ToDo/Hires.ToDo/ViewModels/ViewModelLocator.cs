using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;  
using GalaSoft.MvvmLight.Views;
using Hires.ToDo.Services;
using Hires.ToDo.Views;

namespace Hires.ToDo.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary> 
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var navigationService = new NavigationService();
            navigationService.Configure("SettingPage", typeof(SettingPage));
            navigationService.Configure("MainPage", typeof(MainPage));
            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    // Create design time view services and models
            //}
            //else
            //{
            //    // Create run time view services and models
            //}

            SimpleIoc.Default.Register<INavigationService>(() => navigationService);
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<IPersistationService, PersistationService>();
            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<SettingPageViewModel>();
        }

        public MainPageViewModel MainPageInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainPageViewModel>();
            }
        }

        public SettingPageViewModel SettingPageInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingPageViewModel>();
            }
        }

        // <summary>
        // The cleanup.
        // </summary>
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }

}