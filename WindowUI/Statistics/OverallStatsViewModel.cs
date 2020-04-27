﻿using System;
using System.Collections.Generic;
using Domain.WorkTimeAggregate;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.Statistics
{
    public enum OverallStatsChartTypes
    {
        Applications,Summary,Monitorings,SingleApplication
    }

    public class OverallStatsViewModel : BindableBase, INavigationAware
    {
        private readonly OverallStatsController _controller;

        private SeriesCollection _applicationsSeries = new SeriesCollection();
        private SeriesCollection _singleApplicationSeries = new SeriesCollection();
        private SeriesCollection _summarySeries = new SeriesCollection();
        private int _lowerDays;
        private int _upperDays;
        private int _maxDays;
        private int _minDays;
        private DateTime _maxDate;
        private DateTime _minDate;
        private List<OverallStatsChartTypes> _chartTypes = new List<OverallStatsChartTypes>(){ OverallStatsChartTypes.Applications, OverallStatsChartTypes.Summary, OverallStatsChartTypes.Monitorings, OverallStatsChartTypes.SingleApplication };
        private OverallStatsChartTypes _selectedChartType = OverallStatsChartTypes.Applications;
        private List<WorkTimeViewModel> _monitorings = new List<WorkTimeViewModel>();
        private DateTime _selectedMinDate;
        private DateTime _selectedMaxDate;
        private List<string> _executables;
        private string _selectedExecutable;
        private bool _isShowingStats = true;

        public OverallStatsViewModel(OverallStatsController controller)
        {
            _controller = controller;
        }

        public SeriesCollection ApplicationsSeries
        {
            get => _applicationsSeries;
            set => SetProperty(ref _applicationsSeries, value);
        }

        public SeriesCollection SummarySeries
        {
            get => _summarySeries;
            set => SetProperty(ref _summarySeries, value);
        }

        public SeriesCollection SingleApplicationSeries
        {
            get => _singleApplicationSeries;
            set => SetProperty(ref _singleApplicationSeries, value);
        }

        public List<string> Executables
        {
            get => _executables;
            set => SetProperty(ref _executables, value);
        }

        public string SelectedExecutable
        {
            get => _selectedExecutable;
            set => SetProperty(ref _selectedExecutable, value);
        }

        public int LowerDays
        {
            get => _lowerDays;
            set => SetProperty(ref _lowerDays, value);
        }

        public int UpperDays
        {
            get => _upperDays;
            set => SetProperty(ref _upperDays, value);
        }

        public int MaxDays
        {
            get => _maxDays;
            set => SetProperty(ref _maxDays, value);
        }

        public int MinDays
        {
            get => _minDays;
            set => SetProperty(ref _minDays, value);
        }

        public DateTime MaxDate
        {
            get => _maxDate;
            set => SetProperty(ref _maxDate, value);
        }

        public DateTime MinDate
        {
            get => _minDate;
            set => SetProperty(ref _minDate, value);
        }

        public DateTime SelectedMaxDate
        {
            get => _selectedMaxDate;
            set => SetProperty(ref _selectedMaxDate, value);
        }

        public DateTime SelectedMinDate
        {
            get => _selectedMinDate;
            set => SetProperty(ref _selectedMinDate, value);
        }

        public List<OverallStatsChartTypes> ChartTypes
        {
            get => _chartTypes;
            set => SetProperty(ref _chartTypes, value);
        }

        public OverallStatsChartTypes SelectedChartType
        {
            get => _selectedChartType;
            set => SetProperty(ref _selectedChartType, value);
        }

        public List<WorkTimeViewModel> Monitorings
        {
            get => _monitorings;
            set => SetProperty(ref _monitorings, value);
        }

        public bool IsShowingStats
        {
            get => _isShowingStats;
            set => SetProperty(ref _isShowingStats, value);
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
    }
}