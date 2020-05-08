using System;
using System.Drawing.Printing;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Domain.Services;
using Domain.WorkTimeAggregate;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    internal class State2Configuration
    {
        public int[] RetryDelays { get; set; } = new[] {4000, 6000, 8000, 12_000, 30_000};
        public int FaceDetectionDelayThreshold { get; set; } = 12_000;
    }

    public partial class WMonitorAlghorithm
    {
        internal class State2Service
        {
            private readonly AlghorithmFaceRecognition _faceRecognition;
            private readonly WorkTimeEventService _workTimeEventService;
            private readonly State2Configuration _config;
            private CancellationTokenSource? _cts;

            public State2Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService,
                IConfigurationService configurationService)
            {
                _faceRecognition = faceRecognition;
                _workTimeEventService = workTimeEventService;
                _config = configurationService.Get<State2Configuration>("state2");
            }

            public bool InProgress { get; private set; }
            public void Cancel() => _cts?.Cancel();

            public async Task Enter(State state,
                StateMachine<Triggers, States> sm, WorkTime workTime, WMonitorAlghorithm alghorithm)
            {
                InProgress = true;
                state.CanCapureMouseKeyboard = true;

                _cts = new CancellationTokenSource();
                _workTimeEventService.StartTempChanges();
                _workTimeEventService.StartRecognitionFailure();

                bool faceDetected = false, faceRecognized = false;

                foreach (var timeMs in _config.RetryDelays)
                {
                    (faceDetected, faceRecognized) =
                        await _faceRecognition.RecognizeFace(workTime.User).ConfigureAwait(false);
                    alghorithm.State2Result?.Invoke((faceDetected, faceRecognized));

                    if (!faceDetected && timeMs == _config.FaceDetectionDelayThreshold)
                    {
                        state.CanCapureMouseKeyboard = false;
                        _workTimeEventService.DiscardTempChanges();
                        _workTimeEventService.AddRecognitionFailure(false, faceRecognized);
                        InProgress = false;
                        sm.Next(Triggers.NoFace);
                        return;
                    }

                    if (faceRecognized && faceDetected)
                    {
                        _workTimeEventService.StopRecognitionFailure();
                        _workTimeEventService.CommitTempChanges();
                        InProgress = false;
                        sm.Next(Triggers.FaceRecog);
                        return;
                    }


                    Log.Logger.Debug($"Starting {timeMs} state 2 delay");
                    try
                    {
                        await Task.Delay(timeMs, _cts.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        InProgress = false;
                        return;
                    }
                }

                _workTimeEventService.DiscardTempChanges();
                _workTimeEventService.AddRecognitionFailure(faceDetected, faceRecognized);
                InProgress = false;
                sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
            }
        }
    }
}