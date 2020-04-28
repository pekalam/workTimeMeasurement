﻿using Infrastructure.Messaging;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace NotificationsWpf
{
    public class NotificationsWpfModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var ea = containerProvider.Resolve<IEventAggregator>();

            ea.GetEvent<ShowNotificationEvent>().Subscribe(NotificationService.Show, true);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}