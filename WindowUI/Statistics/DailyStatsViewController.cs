﻿using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Prism.Commands;
using Unity;

namespace WindowUI.Statistics
{
    public interface IDailyStatsViewController
    {
        void Init(DailyStatsViewModel vm);
        ICommand Refresh { get; }
        void Activated();
    }

    public class DailyStatsViewController : IDailyStatsViewController
    {
        private readonly IWorkTimeEsRepository _repository;
        private readonly IAuthenticationService _authenticationService;
        private DailyStatsViewModel? _vm;

        public DailyStatsViewController([Dependency(nameof(WorkTimeEsRepositorDecorator))] IWorkTimeEsRepository repository, IAuthenticationService authenticationService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
            Refresh = new DelegateCommand(UpdateChart);
        }

        public void Init(DailyStatsViewModel vm)
        {
            _vm = vm;

            _vm.PropertyChanged += VmOnPropertyChanged;
            _vm.SeriesPickerViewModel.PropertyChanged += (a, b) => UpdateChart();
        }

        public void Activated()
        {
        }

        public ICommand Refresh { get; }


        public void UpdateChart()
        {
            Debug.Assert(_vm != null);
            var start = new DateTime(_vm.SelectedDate.Year, _vm.SelectedDate.Month, _vm.SelectedDate.Day);
            var end = new DateTime(_vm.SelectedDate.Year, _vm.SelectedDate.Month, _vm.SelectedDate.Day, 23, 59, 59);

            Debug.Assert(_authenticationService.User != null);
            List<WorkTime> workTimes = _repository.FindAll(_authenticationService.User, start.ToUniversalTime(), end.ToUniversalTime());

            if (workTimes.Count == 0)
            {
                _vm.IsShowingStats = false;
                return;
            }
            _vm.IsShowingStats = true;

            var series = workTimes.ToApplicationsPieSeries(_vm.SeriesPickerViewModel).RemoveShort(!_vm.ShowAll);
            _vm.ApplicationsSeries.Clear();
            _vm.ApplicationsSeries.AddRange(series);
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_vm.IsDirty)
            {
                return;
            }
            switch (e.PropertyName)
            {
                case nameof(DailyStatsViewModel.SelectedDate):
                    UpdateChart();
                    break;
                case nameof(DailyStatsViewModel.ShowAll):
                    UpdateChart();
                    break;
            }
        }
    }
}