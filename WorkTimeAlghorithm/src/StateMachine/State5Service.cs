﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    internal class State5Service
    {
        private CancellationTokenSource _cts;

        private int GetRandomDelay()
        {
            var rnd = new Random();
            return rnd.Next(50_000, 120_000);
        }

        public async Task Enter(WMonitorAlghorithm.State state,
            StateMachine<WMonitorAlghorithm.Triggers, WMonitorAlghorithm.States> sm)
        {
            _cts = new CancellationTokenSource();
            state.CanCapureMouseKeyboard = true;

            int timeMs = GetRandomDelay();

            Log.Logger.Debug($"Starting state 5 delay {timeMs}");
            try
            {
                await Task.Delay(timeMs, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            Log.Logger.Debug("State 5 face compare timeout");
            sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
        }

        public void Exit()
        {
            _cts.Cancel();
        }
    }
}