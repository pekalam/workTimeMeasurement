﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Domain.Services;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;

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

        public event Action<(bool faceDetected, bool faceRecognized)> State3DetectionResult; 

        public WMonitorAlghorithm(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService, IConfigurationService configurationService, IMouseKeyboardMonitorService mouseKeyboardMonitor)
        {
            _mouseKeyboardMonitor = mouseKeyboardMonitor;
            _mouseKeyboardMonitor.KeyboardAction.Subscribe(OnKeyboardAction);
            _mouseKeyboardMonitor.MouseMoveAction.Subscribe(OnMouseAction);
            _workTimeEventService = workTimeEventService;
            _state2 = new State2Service(faceRecognition, workTimeEventService, configurationService);
            _state3 = new State3Service(faceRecognition, workTimeEventService, configurationService);
            InitStateMachine();
        }

        public void SetWorkTime(WorkTime workTime)
        {
            _workTime = workTime;
            _workTimeEventService.SetWorkTime(workTime);
        }

        private void OnMouseAction(MonitorEvent ev)
        {
            if (_state.CanCapureMouseKeyboard)
            {
                Debug.WriteLine("Captured mouse action");
                _workTimeEventService.AddMouseEvent(ev);
                _sm.Next(Triggers.MouseMv);
            }
            else
            {
                Debug.WriteLine("Ignoring mouse action");
            }
        }

        private void OnKeyboardAction(MonitorEvent ev)
        {
            if (_state.CanCapureMouseKeyboard)
            {
                Debug.WriteLine("Captured keyboard action");
                _workTimeEventService.AddKeyboardEvent(ev);
                _sm.Next(Triggers.KeyboardMv);
            }
            else
            {
                Debug.WriteLine("Ignoring keyboard action");
            }
        }

        public async void Start()
        {
            _mouseKeyboardMonitor.Start();
            await _sm.NextAsync(Triggers.Start);
        }
    }
}