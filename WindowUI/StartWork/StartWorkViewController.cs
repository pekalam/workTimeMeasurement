using System;
using Prism.Commands;
using Prism.Events;
using WindowUI.Messaging;

namespace WindowUI.StartWork
{
    public interface IStartWorkViewController
    {
        DelegateCommand StartWork { get; }
        public DelegateCommand StopWork { get; }
        public DelegateCommand TogglePauseWork { get; }
    }

    public class StartWorkViewController : IStartWorkViewController
    {
        private StartWorkViewModel _vm;
        private readonly IEventAggregator _ea;
        private readonly WorkTimeModuleService _workTimeModuleService;
        private bool _stopRequested = false;
        private bool _pauseRequested = false;

        public StartWorkViewController(WorkTimeModuleService workTimeModuleService, IEventAggregator ea)
        {
            _workTimeModuleService = workTimeModuleService;
            _ea = ea;
            StartWork = new DelegateCommand(OnStartWorkExecute);
            StopWork = new DelegateCommand(OnStopWorkExecute, () => !_pauseRequested && !_stopRequested && (!_vm?.IsPaused ?? true));
            TogglePauseWork = new DelegateCommand(TogglePauseExecute, () => !_pauseRequested && !_stopRequested);
        }

        private void RaiseCanExecChanged()
        {
            StopWork.RaiseCanExecuteChanged();
            TogglePauseWork.RaiseCanExecuteChanged();
        }

        private async void TogglePauseExecute()
        {
            _pauseRequested = true;
            RaiseCanExecChanged();
            if (!_vm.IsPaused)
            {
                _workTimeModuleService.Resume();
            }
            else
            {
                await _workTimeModuleService.Pause();
            }
            _pauseRequested = false;
            RaiseCanExecChanged();
        }

        private async void OnStopWorkExecute()
        {
            if (_vm.IsPaused)
            {
                _workTimeModuleService.Resume();
            }
            _stopRequested = true;
            RaiseCanExecChanged();
            await _workTimeModuleService.Stop();
            _vm.Started = _stopRequested = false;
            RaiseCanExecChanged();
        }

        private void OnStartWorkExecute()
        {
            if (_vm.HasErrors)
            {
                return;
            }

            if (_vm.AutoStart && _vm.StartDate < DateTime.Now)
            {
                _vm.StartDate = DateTime.Now;
            }

            DateTime? start;
            if (_vm.AutoStart)
            {
                start = null;
            }
            else
            {
                start = _vm.StartDate?.ToUniversalTime();
            }
            DateTime? end = _vm.EndDate?.ToUniversalTime();

            _workTimeModuleService.StartNew(start, end.Value);
            _vm.Started = true;
        }

        private void SetAlgorithmStarted()
        {
            _vm.EndDate = _workTimeModuleService.CurrentWorkTime.EndDate.ToLocalTime();
            _vm.Started = true;
        }

        public void Init(StartWorkViewModel vm)
        {
            _vm = vm;
            _ea.GetEvent<MonitoringRestored>().Subscribe(_ => SetAlgorithmStarted(), true);
        }

        public DelegateCommand StartWork { get; private set; }
        public DelegateCommand StopWork { get; private set; }
        public DelegateCommand TogglePauseWork { get; private set; }
    }
}