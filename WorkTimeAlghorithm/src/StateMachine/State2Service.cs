﻿using System;
using System.Net.Mail;
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

            public State2Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService,
                IConfigurationService configurationService)
            {
                _faceRecognition = faceRecognition;
                _workTimeEventService = workTimeEventService;
                _config = configurationService.Get<State2Configuration>("state2");
            }


            public async Task Enter(State state,
                StateMachine<Triggers, States> sm, WorkTime workTime, WMonitorAlghorithm alghorithm)
            {
                state.CanCapureMouseKeyboard = true;

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
                        sm.Next(Triggers.NoFace);
                        return;
                    }

                    if (faceRecognized && faceDetected)
                    {
                        _workTimeEventService.StopRecognitionFailure();
                        _workTimeEventService.CommitTempChanges();
                        sm.Next(Triggers.FaceRecog);
                        return;
                    }


                    Log.Logger.Debug($"Starting {timeMs} state 2 delay");
                    await Task.Delay(timeMs).ConfigureAwait(false);
                }

                _workTimeEventService.DiscardTempChanges();
                _workTimeEventService.AddRecognitionFailure(faceDetected, faceRecognized);
                sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
            }

            public void Exit(WMonitorAlghorithm.Triggers trigger)
            {
            }
        }
    }
}