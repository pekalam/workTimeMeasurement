﻿using System;
using System.Net.Mime;
using System.Windows;
using Domain.WorkTimeAggregate;
using Prism.Events;
using UI.Common;
using UI.Common.Notifications;
using WindowUI.Messaging;
using WMAlghorithm.Services;

namespace WindowUI
{

    internal class AlghorithmNotificationsAdapter : IAlghorithmNotificationsPort
    {
        private readonly IEventAggregator _ea;

        private int _state3Error;
        private int _state2Error;

        public AlghorithmNotificationsAdapter(IEventAggregator ea)
        {
            _ea = ea;
        }

        public void OnAlgorithmStopped()
        {
            _ea.GetEvent<HideNotificationsEvent>().Publish();
            if (WindowModuleStartupService.ShellWindow.Visibility == Visibility.Visible)
            {
                _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
                {
                    Title = "Monitoring stopped",
                    Msg = "",
                    Scenario = NotificationScenario.Information
                });
            }

        }

        public void AlghorithmOnState2Result((bool faceDetected, bool faceRecognized) args)
        {
            _state2Error += !args.faceDetected || !args.faceRecognized ? 1 : 0;

            if (args.faceDetected && args.faceRecognized)
            {
                if (_state2Error >= 2)
                {
                    _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
                    {
                        Title = "Face recognized",
                        Msg = "Continue your work",
                        Scenario = NotificationScenario.Information,
                    });
                }

                _state2Error = 0;
                return;
            }

            if (_state2Error >= 2)
            {
                _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
                {
                    Title = args.faceDetected ? "Cannot recognize face" : "Cannot detect face",
                    Msg = "Look at front of screen",
                    Scenario = NotificationScenario.Warning,
                });
            }
        }

        public void AlghorithmOnState3Result((bool faceDetected, bool faceRecognized) args)
        {
            _state3Error += !args.faceDetected || !args.faceRecognized ? 1 : 0;


            if (args.faceDetected && args.faceRecognized)
            {
                if (_state3Error >= 1)
                {
                    _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
                    {
                        Title = "Face recognized",
                        Msg = "Continue your work",
                        Scenario = NotificationScenario.Information,
                    });
                }

                _state3Error = 0;
                return;
            }


            if (_state3Error >= 1)
            {
                _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
                {
                    Title = args.faceDetected ? "Cannot recognize face" : "Cannot detect face",
                    Msg = "Look at front of screen",
                    Scenario = NotificationScenario.WarningTrigger,
                    Length = TimeSpan.FromSeconds(20)
                });
            }
        }

        public void Reset()
        {
            _state2Error = _state3Error = 0;
        }

        public void OnRestored(WorkTime workTime)
        {
            _ea.GetEvent<MonitoringRestored>().Publish(workTime);
            _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
            {
                Title = "Continuing stopped monitoring",
                Msg = "",
                Scenario = NotificationScenario.Information,
            });
        }
    }
}