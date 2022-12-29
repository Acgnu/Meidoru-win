/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:AcgnuX"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace AcgnuX.Source.ViewModel
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

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<DnsManageViewModel>();
            SimpleIoc.Default.Register<PwdRepositoryViewModel>();
            SimpleIoc.Default.Register<PianoScoreDownloadRecordViewModel>();
            SimpleIoc.Default.Register<Tan8PlayerViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<Tan8SheetReponsitoryViewModel>();
            SimpleIoc.Default.Register<DeviceSyncViewModel>();
            SimpleIoc.Default.Register<DeviceSyncPathConfigDialogViewModel>();
            SimpleIoc.Default.Register<ContactManageViewModel>();
        }

        public MainWindowViewModel Main { get { return ServiceLocator.Current.GetInstance<MainWindowViewModel>(); } }

        public DnsManageViewModel DnsManage { get { return ServiceLocator.Current.GetInstance<DnsManageViewModel>(); } }

        public PwdRepositoryViewModel PwdRepository { get { return ServiceLocator.Current.GetInstance<PwdRepositoryViewModel>(); } }

        public Tan8SheetReponsitoryViewModel SheetRepository { get { return ServiceLocator.Current.GetInstance<Tan8SheetReponsitoryViewModel>(); } }

        public PianoScoreDownloadRecordViewModel Tan8DownloadRecord { get { return ServiceLocator.Current.GetInstance<PianoScoreDownloadRecordViewModel>(); } }

        public Tan8PlayerViewModel Tan8Player { get { return ServiceLocator.Current.GetInstance<Tan8PlayerViewModel>(); } }

        public SettingsViewModel SettingsMajor { get { return ServiceLocator.Current.GetInstance<SettingsViewModel>(); } }

        public DeviceSyncViewModel DeviceSync { get { return ServiceLocator.Current.GetInstance<DeviceSyncViewModel>(); } }

        public DeviceSyncPathConfigDialogViewModel DeviceSyncPathConfig { get { return ServiceLocator.Current.GetInstance<DeviceSyncPathConfigDialogViewModel>(); } }

        public ContactManageViewModel ContactMng { get { return ServiceLocator.Current.GetInstance<ContactManageViewModel>(); } }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}