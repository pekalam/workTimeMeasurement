using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using Infrastructure.Messaging;
using Prism.Events;
using WindowUI.Messaging;
using WorkTimeAlghorithm.StateMachine;

namespace WindowUI
{
    public class WorkTimeModuleService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly WorkTimeBuildService _buildService;
        private readonly WMonitorAlghorithm _alghorithm;
        private readonly WorkTimeRestoreService _restoreService;
        private readonly IWorkTimeEsRepository _repository;
        private readonly MonitorAlghorithmNotifications _monitorAlghorithmNotifications;
        private readonly IEventAggregator _ea;

        public WorkTimeModuleService(IAuthenticationService authenticationService, WorkTimeBuildService buildService,
            WMonitorAlghorithm alghorithm, IWorkTimeEsRepository repository, WorkTimeRestoreService restoreService
            , IEventAggregator ea)
        {
            _authenticationService = authenticationService;
            _buildService = buildService;
            _alghorithm = alghorithm;
            _repository = repository;
            _restoreService = restoreService;
            _ea = ea;
            ea.GetEvent<AppShuttingDownEvent>().Subscribe(() => _restoreService.SetInterrupted(CurrentWorkTime), true);

            _monitorAlghorithmNotifications = new MonitorAlghorithmNotifications(alghorithm, ea);
        }


        public bool AlgorithmStarted { get; private set; }
        public WorkTime? CurrentWorkTime { get; private set; }

        public WMonitorAlghorithm Alghorithm => _alghorithm;

        public void StartNew(DateTime? start, DateTime end)
        {
            var created = _buildService.CreateStartedManually(_authenticationService.User, end, true);
            StartAlgorithm(created);
        }

        private void StartAlgorithm(WorkTime workTime)
        {
            CurrentWorkTime = workTime;
            _alghorithm.SetWorkTime(workTime);
            _alghorithm.Start();
            AlgorithmStarted = true;
        }

        public async Task Pause()
        {
            await _alghorithm.Pause();
            CurrentWorkTime.Pause();
        }

        public void Resume()
        {
            CurrentWorkTime.Resume();
            _alghorithm.Resume();
        }

        public async Task Stop()
        {
            await _alghorithm.Stop();
            CurrentWorkTime.Stop();
            _monitorAlghorithmNotifications.Reset();
            _repository.Save(CurrentWorkTime);
            CurrentWorkTime.MarkPendingEventsAsHandled();
        }

        public bool TryRestore()
        {
            var user = _authenticationService.User;
            if (_restoreService.Restore(user, out var restored))
            {
                StartAlgorithm(restored);
                _ea.GetEvent<MonitoringRestored>().Publish(this);
                _monitorAlghorithmNotifications.OnRestored();
                return true;
            }

            return false;
        }
    }
}