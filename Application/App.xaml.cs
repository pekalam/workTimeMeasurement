using System;
using System.Diagnostics;
using System.Windows;
using CommonServiceLocator;
using Infrastructure;
using Infrastructure.Messaging;
using NotificationsW8;
using NotificationsWpf;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using WindowUI;

namespace Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public App()
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
#if MSIX_RELEASE
            moduleCatalog.AddModule<NotificationsW8Module>();
#else
            moduleCatalog.AddModule<NotificationsWpfModule>();
#endif
            moduleCatalog.AddModule<InfrastructureModule>();
            moduleCatalog.AddModule<WindowUiModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<AppShuttingDownEvent>().Publish();
            base.OnSessionEnding(e);
        }
    }
}
