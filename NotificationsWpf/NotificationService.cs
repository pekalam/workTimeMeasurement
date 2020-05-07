using System;
using CommonServiceLocator;
using Infrastructure.Messaging;
using Notifications.Wpf;
using Prism.Events;

namespace NotificationsWpf
{
    internal static class NotificationService
    {
        private static NotificationType GetNotificationType(NotificationConfig config)
        {
            switch (config.Scenario)
            {
                case NotificationScenario.Information:
                    return NotificationType.Information;
                case NotificationScenario.Warning:
                    return NotificationType.Warning;
                case NotificationScenario.WarningTrigger:
                    return NotificationType.Warning;
                default:
                    return NotificationType.Information;
            }
        }

        public static void Show(NotificationConfig config)
        {
            var notificationManager = new NotificationManager();

            if (config.Scenario == NotificationScenario.WarningTrigger)
            {
                notificationManager.Show(
                    new RecogTriggerViewModel(ServiceLocator.Current.GetInstance<IEventAggregator>())
                    {
                        Content = new NotificationContent
                        {
                            Title = config.Title,
                            Message = config.Msg,
                            Type = GetNotificationType(config)
                        }
                    }, expirationTime:TimeSpan.FromSeconds(38));
            }
            else
            {
                notificationManager.Show(new NotificationContent
                {
                    Title = config.Title,
                    Message = config.Msg,
                    Type = GetNotificationType(config)
                });
            }
        }
    }
}