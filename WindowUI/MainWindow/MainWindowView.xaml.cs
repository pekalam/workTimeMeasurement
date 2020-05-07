using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Infrastructure;
using Infrastructure.Repositories;
using MahApps.Metro.Controls;
using Prism.Commands;
using Prism.Regions;
using WindowUI.FaceInitialization;
using WindowUI.Profile;
using WindowUI.Settings;
using WindowUI.StartWork;
using WindowUI.Statistics;

namespace WindowUI.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindowView
    /// </summary>
    public partial class MainWindowView : UserControl
    {
        public MainWindowView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                GlobalExceptionHandler.Handle(e);
            }
            WindowUiModuleCommands.NavigateProfile.RegisterCommand(new DelegateCommand(() =>
                {
                    HamburgerMenuControl.SelectedOptionsIndex = 0;
                }));
        }

        private void HamburgerMenuControl_HamburgerButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!HamburgerMenuControl.IsPaneOpen)
            {
                HamburgerMenuControl.Width = HamburgerMenuControl.OpenPaneLength;
            }
            else
            {
                HamburgerMenuControl.Width = HamburgerMenuControl.CompactPaneLength;
            }
        }

        private void HamburgerMenuControl_ItemInvoked(object sender, MahApps.Metro.Controls.HamburgerMenuItemInvokedEventArgs args)
        {
            var menuItem = (args.InvokedItem as HamburgerMenuItem);
            Debug.Assert(menuItem != null);
            Debug.Assert(menuItem.Tag != null);
            var navItemVm = (menuItem.Tag as NavigationItemViewModel);
            
            if (navItemVm == null)
            {
                throw new ArgumentException("Null tag value of menu item");
            }

            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            switch (navItemVm.NavigationItem)
            {
                case NavigationItems.StartMonitor:
                    regionManager.Regions[MainWindowRegions.MainContentRegion].RequestNavigate(nameof(StartWorkView));
                    break;
                case NavigationItems.Statistics:
                    regionManager.Regions[MainWindowRegions.MainContentRegion].RequestNavigate(nameof(StatisticsView));
                    break;
                case NavigationItems.Profile:
                    regionManager.Regions[MainWindowRegions.MainContentRegion].RequestNavigate(nameof(ProfileView));
                    break;
                case NavigationItems.Settings:
                    regionManager.Regions[MainWindowRegions.MainContentRegion].RequestNavigate(nameof(SettingsView));
                    break;
            }

        }
    }
}
