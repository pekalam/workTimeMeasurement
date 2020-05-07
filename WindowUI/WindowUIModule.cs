using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using Domain.Repositories;
using Infrastructure.Messaging;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Unity;
using WindowUI.FaceInitialization;
using WindowUI.LoginWindow;
using WindowUI.MainWindow;
using WindowUI.Profile;
using WindowUI.RepoProxy;
using WindowUI.Settings;
using WindowUI.SplashScreen;
using WindowUI.StartWork;
using WindowUI.Statistics;
using WindowUI.TriggerRecognition;

namespace WindowUI
{
    public static class WindowUiModuleCommands
    {
        public static CompositeCommand NavigateProfile { get; }

        static WindowUiModuleCommands()
        {
            NavigateProfile = new CompositeCommand();
        }
    }

    public class WindowUiModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var ea = containerProvider.GetContainer().Resolve<IEventAggregator>();
            ea.GetEvent<FaceRecogTriggeredEvent>().Subscribe(
                () =>
                {
                    var service = containerProvider.Resolve<TriggerRecognitionNavigation>();
                    service.NaviateToTriggerRecogView();
                }, true);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoginView>();
            containerRegistry.RegisterForNavigation<MainWindowView>();
            containerRegistry.RegisterForNavigation<FaceInitializationView>();
            containerRegistry.RegisterForNavigation<StartWorkView>();
            containerRegistry.RegisterForNavigation<StatisticsView>();
            containerRegistry.RegisterForNavigation<ProfileView>();
            containerRegistry.RegisterForNavigation<TriggerRecognitionView>();
            containerRegistry.RegisterForNavigation<SplashScreenView>();
            containerRegistry.RegisterForNavigation<SettingsView>();

            containerRegistry.GetContainer()
                .RegisterType<ITriggerRecognitionController, TriggerRecognitionController>();

            containerRegistry.GetContainer()
                .RegisterType<IDailyStatsViewController, DailyStatsViewController>();

            containerRegistry.GetContainer()
                .RegisterType<IFaceInitializationController, FaceInitializationController>();

            containerRegistry.GetContainer()
                .RegisterType<IMainViewController, MainViewController>();

            containerRegistry.GetContainer()
                .RegisterType<ILoginViewController, LoginViewController>();

            containerRegistry.GetContainer()
                .RegisterType<IProfileViewController, ProfileViewController>();

            containerRegistry.GetContainer().RegisterType<IOverallStatsController, OverallStatsController>();

            containerRegistry.GetContainer().RegisterSingleton<WorkTimeModuleService>();

            containerRegistry.GetContainer()
                .RegisterInstance(ServiceLocator.Current.GetInstance<WindowModuleStartupService>());

            containerRegistry.GetContainer()
                .RegisterType<IWorkTimeEsRepository, WorkTimeEsRepositorDecorator>(nameof(WorkTimeEsRepositorDecorator));
        }
    }
}