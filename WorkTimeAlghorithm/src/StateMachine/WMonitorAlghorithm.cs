using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Domain.Services;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using OpenCvSharp;

namespace WorkTimeAlghorithm.StateMachine
{
    public partial class WMonitorAlghorithm
    {
        private readonly IMouseKeyboardMonitorService _mouseKeyboardMonitor;
        private readonly WorkTimeEventService _workTimeEventService;
        private readonly State2Service _state2;
        private readonly State3Service _state3;
        private readonly State5Service _state5 = new State5Service();
        private WorkTime _workTime;

        public event Action<(bool faceDetected, bool faceRecognized)> State3Result;
        public event Action<(bool faceDetected, bool faceRecognized)> State2Result;

        private IDisposable _keyboardSub;
        private IDisposable _mouseSub;

        public WMonitorAlghorithm(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService, IConfigurationService configurationService, IMouseKeyboardMonitorService mouseKeyboardMonitor)
        {
            _mouseKeyboardMonitor = mouseKeyboardMonitor;
            InitSubscriptions();
            _workTimeEventService = workTimeEventService;
            _state2 = new State2Service(faceRecognition, workTimeEventService, configurationService);
            _state3 = new State3Service(faceRecognition, workTimeEventService, configurationService);
        }

        public bool ManualRecog => _sm.CurrentState.Name == States.MANUAL;

        private void InitSubscriptions()
        {
            _keyboardSub = _mouseKeyboardMonitor.KeyboardAction.Subscribe(OnKeyboardAction);
            _mouseSub = _mouseKeyboardMonitor.MouseMoveAction.Subscribe(OnMouseAction);
        }

        public void SetWorkTime(WorkTime workTime)
        {
            _workTime = workTime;
            _workTimeEventService.SetWorkTime(workTime);
        }

        private void OnMouseAction(MonitorEvent ev)
        {
            if (_state.CanCapureMouseKeyboard && !_workTime.Paused && !_workTime.Stopped)
            {
                Debug.WriteLine("Captured mouse action");
                _workTimeEventService.AddMouseEvent(ev);
                _sm.Next(Triggers.MouseMv);
            }
            else
            {
                if (_workTime.Stopped)
                {
                    Stop();
                }
                Debug.WriteLine("Ignoring mouse action");
            }
        }

        private void OnKeyboardAction(MonitorEvent ev)
        {
            if (_state.CanCapureMouseKeyboard && !_workTime.Paused && !_workTime.Stopped)
            {
                Debug.WriteLine("Captured keyboard action");
                _workTimeEventService.AddKeyboardEvent(ev);
                _sm.Next(Triggers.KeyboardMv);
            }
            else
            {
                if (_workTime.Stopped)
                {
                    Stop();
                }
                Debug.WriteLine("Ignoring keyboard action");
            }
        }

        public async void Start()
        {
            InitSubscriptions();
            InitStateMachine();
            _mouseKeyboardMonitor.Start();
            await _sm.NextAsync(Triggers.Start);
        }

        public async Task Pause()
        {
            await StopStateServices();
            _sm.Next(Triggers.Pause);
            _state.CanCapureMouseKeyboard = true;
        }

        public void Resume()
        {
            _sm.NextAsync(Triggers.Resume);
            _workTimeEventService.ResetLastEvents();
        }

        private Task StopStateServices()
        {
            return Task.Run(async () =>
            {
                do
                {
                    _state2.Cancel();
                    _state3.Cancel();
                    _state5.Cancel();
                    await Task.Delay(200);
                } while (_state2.InProgress || _state3.InProgress || _state5.InProgress);
            });
        }

        public Task Stop()
        {
            _state.CanCapureMouseKeyboard = false;
            _keyboardSub.Dispose();
            _mouseSub.Dispose();
            _mouseKeyboardMonitor.Stop();

            var task= StopStateServices();

#if DEV_MODE
            task = task.ContinueWith(_ => _vis.Dispose());
#endif

            return task;
        }


        public void StartManualFaceRecog()
        {
            _sm.Next(Triggers.ManualTrigger);
        }

        public void CancelManualFaceRecog()
        {
            _sm.Next(Triggers.ManualCancel);
        }

        public void SetFaceRecog()
        {
            _sm.Next(Triggers.FaceRecog);
        }
    }
}