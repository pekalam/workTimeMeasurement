using Infrastructure.Messaging;
using Prism.Events;
using WorkTimeAlghorithm.StateMachine;

namespace WindowUI
{
    internal class MonitorAlghorithmNotifications
    {
        private readonly WMonitorAlghorithm _alghorithm;
        private readonly IEventAggregator _ea;

        private int _state3Error;
        private int _state2Error;

        public MonitorAlghorithmNotifications(WMonitorAlghorithm alghorithm, IEventAggregator ea)
        {
            _alghorithm = alghorithm;
            _ea = ea;
            _alghorithm.State3Result += AlghorithmOnState3Result;
            _alghorithm.State2Result += AlghorithmOnState2Result;
        }

        private void AlghorithmOnState2Result((bool faceDetected, bool faceRecognized) args)
        {
            _state2Error += !args.faceDetected || !args.faceRecognized ? 1 : 0;

            if (args.faceDetected && args.faceRecognized)
            {
                if (_state2Error >= 2)
                {
                    _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
                    {
                        Title = "Face recognized",
                        Msg = "Continue Your work",
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
                    Title = !args.faceRecognized ? "Cannot recognize face" : "Cannot detect face",
                    Msg = "Look at front of screen",
                    Scenario = NotificationScenario.Warning,
                });
            }
        }

        private void AlghorithmOnState3Result((bool faceDetected, bool faceRecognized) args)
        {
            _state3Error += !args.faceDetected || !args.faceRecognized ? 1 : 0;


            if (args.faceDetected && args.faceRecognized)
            {
                if (_state3Error >= 1)
                {
                    _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
                    {
                        Title = "Face recognized",
                        Msg = "Continue Your work",
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
                    Title = !args.faceRecognized ? "Cannot recognize face" : "Cannot detect face",
                    Msg = "Look at front of screen",
                    Scenario = NotificationScenario.WarningTrigger,
                });
            }
        }

        public void Reset()
        {
            _state2Error = 0;
        }

        public void OnRestored()
        {
            _ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
            {
                Title = "Continuing stopped monitoring",
                Msg = "",
                Scenario = NotificationScenario.Information,
            });
        }
    }
}