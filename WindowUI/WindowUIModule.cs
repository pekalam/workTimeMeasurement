﻿using CommonServiceLocator;
using Domain.Repositories;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using UI.Common;
using UI.Common.Messaging;
using Unity;
using WindowUI.LoginWindow;
using WindowUI.MainWindow;
using WindowUI.Profile;
using WindowUI.ProfileInit;
using WindowUI.Settings;
using WindowUI.SplashScreen;
using WindowUI.StartWork;
using WindowUI.Statistics;
using WindowUI.TriggerRecognition;
using WMAlghorithm.Services;

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


            ea.GetEvent<AppShuttingDownEvent>().Subscribe(() =>
            {
                ServiceLocator.Current.GetInstance<AlgorithmService>().Shutdown();
            }, true);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoginView>();
            containerRegistry.RegisterForNavigation<MainWindowView>();
            containerRegistry.RegisterForNavigation<ProfileInitView>();
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
                .RegisterType<IProfileInitController, ProfileInitController>();

            containerRegistry.GetContainer()
                .RegisterType<IMainViewController, MainViewController>();

            containerRegistry.GetContainer()
                .RegisterType<ILoginViewController, LoginViewController>();

            containerRegistry.GetContainer()
                .RegisterType<IProfileViewController, ProfileViewController>();

            containerRegistry.GetContainer().RegisterType<IOverallStatsController, OverallStatsController>();

            containerRegistry.GetContainer()
                .RegisterType<IAlghorithmNotificationsPort, AlghorithmNotificationsAdapter>();

            containerRegistry.GetContainer()
                .RegisterInstance(ServiceLocator.Current.GetInstance<WindowModuleStartupService>());

            containerRegistry.GetContainer()
                .RegisterType<IWorkTimeEsRepository, WorkTimeEsRepositorDecorator>(nameof(WorkTimeEsRepositorDecorator));
        }
    }
}