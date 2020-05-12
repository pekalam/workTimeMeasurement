﻿using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Prism;

namespace WindowUI.StartWork
{
    public class StartWorkViewModel : BindableBase, INavigationAware, INotifyDataErrorInfo
    {
        private readonly StartWorkViewController _controller;
        private bool _autoStart;
        private bool _started;
        private DateTime? _startDate = DateTime.Now;
        private DateTime? _endDate = DateTime.Now.AddHours(1);
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private TimeSpan _timerDate;
        private bool _isPaused;
        private bool _startCanExec;

        public StartWorkViewModel(StartWorkViewController controller)
        {
            _controller = controller;
            StartWork = controller.StartWork.ObservesCanExecute(() => StartCanExec);
            StopWork = controller.StopWork;
            TogglePauseWork = controller.TogglePauseWork;
            _timer.Tick += TimerOnTick;
            _timer.Interval = TimeSpan.FromSeconds(1);
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            TimerDate = EndDate.Value.Subtract(DateTime.Now);
        }

        public DelegateCommand StartWork { get; set; }
        public DelegateCommand StopWork { get; }
        public DelegateCommand TogglePauseWork { get; }

        public bool StartCanExec
        {
            get => _startCanExec;
            set => SetProperty(ref _startCanExec, value);
        }

        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                SetProperty(ref _autoStart, value);
                if (value && StartDate < DateTime.Now)
                {
                    StartDate = DateTime.Now;
                }
            }
        }

        public bool Started
        {
            get => _started;
            set
            {
                SetProperty(ref _started, value);
                if (value)
                {
                    SetTimerDate(EndDate.Value);
                }
            }
        }

        public void SetTimerDate(DateTime endDate)
        {
            if (AutoStart)
            {
                TimerDate = endDate.Subtract(StartDate.Value);
            }
            else
            {
                TimerDate = endDate.Subtract(DateTime.Now);
            }
            _timer.Start();
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                var endHad = EndHasError;
                SetProperty(ref _startDate, value);
                StartCanExec = !HasErrors;
                if (endHad && !EndHasError)
                {
                    RaisePropertyChanged(nameof(EndDate));
                }
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                var startHad = StartHasError;
                SetProperty(ref _endDate, value);
                StartCanExec = !HasErrors;
                if (startHad && !StartHasError)
                {
                    RaisePropertyChanged(nameof(StartDate));
                }
                if (StartDate < DateTime.Now)
                {
                    StartDate = DateTime.Now;
                }
            }
        }

        public TimeSpan TimerDate
        {
            get => _timerDate;
            set
            {
                SetProperty(ref _timerDate, value);
                if (value.TotalMilliseconds <= 0)
                {
                    Started = false;
                    _timer.Stop();
                    _controller.OnTimerStopped();
                }
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private bool StartHasError => AutoStart && (StartDate >= EndDate || (StartDate.Value.Subtract(DateTime.Now)).TotalSeconds < -5);
        private bool EndHasError => EndDate <= StartDate || EndDate <= DateTime.Now;

        public IEnumerable GetErrors(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(StartDate):
                    return StartHasError
                        ? new List<string>() {"Invalid start date"}
                        : Enumerable.Empty<string>();
                case nameof(EndDate):
                    return EndHasError ? new List<string>() {"Invalid end date"} : Enumerable.Empty<string>();
            }

            return Enumerable.Empty<string>();
        }

        public bool HasErrors => StartHasError || EndHasError;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public void Validate()
        {
            StartDate = StartDate;
            EndDate = EndDate;
            //RaisePropertyChanged(nameof(StartDate));
            //RaisePropertyChanged(nameof(EndDate));
        }
    }
}