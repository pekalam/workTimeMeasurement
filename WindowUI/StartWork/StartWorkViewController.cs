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
        public DelegateCommand PauseWork { get; }
        public DelegateCommand ResumeWork { get; }
    }

    public class StartWorkViewController : IStartWorkViewController
    {
        private StartWorkViewModel _vm;
        private readonly IEventAggregator _ea;
        private readonly WorkTimeModuleService _workTimeModuleService;

        public StartWorkViewController(WorkTimeModuleService workTimeModuleService, IEventAggregator ea)
        {
            _workTimeModuleService = workTimeModuleService;
            _ea = ea;
            StartWork = new DelegateCommand(OnStartWorkExecute, CanExecuteMethod);
            StopWork = new DelegateCommand(OnStopWorkExecute);
            PauseWork = new DelegateCommand(OnPauseWorkExecute);
            ResumeWork = new DelegateCommand(OnResumeWorkExecute);
        }

        private void OnResumeWorkExecute()
        {
            _workTimeModuleService.Resume();
        }

        private void OnPauseWorkExecute()
        {
            _workTimeModuleService.Pause();
        }

        private void OnStopWorkExecute()
        {
            _workTimeModuleService.Stop();
            _vm.Started = false;
        }

        private bool CanExecuteMethod()
        {
            return true;
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
        public DelegateCommand PauseWork { get; private set; }
        public DelegateCommand ResumeWork { get; private set; }
    }
}