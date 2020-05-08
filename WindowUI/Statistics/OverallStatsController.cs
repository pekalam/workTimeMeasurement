using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using LiveCharts;
using LiveCharts.Wpf;
using Unity;
using WindowUI.RepoProxy;

namespace WindowUI.Statistics
{
    internal static class WorkTimeStatsExtensions
    {
        public static List<PieSeries> ToApplicationsPieSeries(this List<WorkTime> selected, SeriesPickerViewModel seriesPicker)
        {
            return ToApplicationsPieSeries(selected, seriesPicker.ShowMouse, seriesPicker.ShowKeyboard,
                seriesPicker.ShowWatchingScreen, seriesPicker.ShowAway);
        }

        public static List<PieSeries> ToApplicationsPieSeries(this List<WorkTime> selected, bool mouse = true, bool keyboard = true, bool watchingScr = true, bool away = true)
        {
            var series =
                new List<PieSeries>();

            if (mouse)
            {
                series = series.Concat(selected.SelectMany(w => w.MouseActionEvents)
                    .GroupBy(a => a.MkEvent.Executable)
                    .Select(g => g.AsEnumerable().ToPieSeries(g.Key, true))
                    .SelectMany(s => s)).ToList();
            }

            if (keyboard)
            {
                series = series.Concat(selected.SelectMany(w => w.KeyboardActionEvents)
                    .GroupBy(a => a.MkEvent.Executable)
                    .Select(g => g.AsEnumerable().ToPieSeries(g.Key, true))
                    .SelectMany(s => s)).ToList();
            }

            if (watchingScr)
            {
                series = series.Concat(selected.SelectMany(w => w.UserWatchingScreen)
                    .GroupBy(a => a.Executable)
                    .Select(g => g.AsEnumerable().ToPieSeries(g.Key, true))
                    .SelectMany(s => s)).ToList();
            }

            if (away)
            {
                series = series.Concat(selected.SelectMany(w => w.FaceRecognitionFailures).ToPieSeries("Unknown", true))
                    .ToList();
            }

            return series;
        }

        public static List<PieSeries> RemoveShort(this List<PieSeries> series,bool remove)
        {
            if (remove)
            {
                var avg = series.Sum(s => (long) s.Values[0]) / (float)series.Count;
                return series.Where(s => ((long) s.Values[0]) * 100 / avg > 50.0).ToList();
            }

            return series;
        } 
    }

    public interface IOverallStatsController
    {
        void Init(OverallStatsViewModel vm);
    }

    public class OverallStatsController : IOverallStatsController
    {
        private OverallStatsViewModel _vm;
        private List<WorkTime> _workTimes;
        private readonly IWorkTimeEsRepository _repository;
        private readonly IAuthenticationService _authenticationService;
        private DateTime _startDate;
        private DateTime _endDate;

        public OverallStatsController([Dependency(nameof(WorkTimeEsRepositorDecorator))] IWorkTimeEsRepository repository,
            IAuthenticationService authenticationService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
        }

        private void SetupDateSlider()
        {
            var first = _workTimes.First();
            var last = _workTimes.Last();


            _startDate = new DateTime(first.DateCreated.Year, first.DateCreated.Month, first.DateCreated.Day);
            _endDate = new DateTime(last.EndDate.Year, last.EndDate.Month, last.EndDate.Day, 23, 59, 59);

            var diff = _endDate - _startDate;


            _vm.MaxDays = (int) diff.TotalDays;
            _vm.MinDays = 0;
            _vm.UpperDays = _vm.MaxDays;
            _vm.LowerDays = _vm.MinDays;
            _vm.MinDate = _startDate;
            _vm.MaxDate = _endDate;
            _vm.SelectedMinDate = _startDate;
            _vm.SelectedMaxDate = _endDate;
        }

        private void UpdateApplicationsSeries(List<WorkTime> selected)
        {
            var series = selected.ToApplicationsPieSeries(_vm.SeriesPickerViewModel).RemoveShort(!_vm.ShowAll);

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                _vm.ApplicationsSeries.Clear();
                _vm.ApplicationsSeries.AddRange(series);
            }, DispatcherPriority.Input);
        }

        private void UpdateSummarySeries(List<WorkTime> selected)
        {
            var series = selected.SelectMany(w => w.MouseActionEvents).ToPieSeries()
                .Concat(selected.SelectMany(w => w.KeyboardActionEvents).ToPieSeries())
                .Concat(selected.SelectMany(w => w.UserWatchingScreen).ToPieSeries())
                .Concat(selected.SelectMany(w => w.FaceRecognitionFailures).ToPieSeries()).ToList();

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                _vm.SummarySeries.Clear();
                _vm.SummarySeries.AddRange(series);
            }, DispatcherPriority.Input);
        }

        private void UpdateSingleApplicationSeries(List<WorkTime> selected)
        {
            _vm.Executables = selected.SelectMany(w => w.MouseActionEvents).Select(w => w.MkEvent.Executable)
                .Concat(selected.SelectMany(w => w.KeyboardActionEvents).Select(w => w.MkEvent.Executable))
                .Concat(selected.SelectMany(w => w.UserWatchingScreen).Select(w => w.Executable))
                .Distinct().ToList();

            var series = selected.SelectMany(w => w.MouseActionEvents)
                .Where(a => a.MkEvent.Executable == _vm.SelectedExecutable).ToPieSeries()
                .Concat(selected.SelectMany(w => w.KeyboardActionEvents)
                    .Where(a => a.MkEvent.Executable == _vm.SelectedExecutable).ToPieSeries())
                .Concat(selected.SelectMany(w => w.UserWatchingScreen)
                    .Where(a => a.Executable == _vm.SelectedExecutable).ToPieSeries()).ToList();

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                _vm.SingleApplicationSeries.Clear();
                _vm.SingleApplicationSeries.AddRange(series);
            }, DispatcherPriority.Input);
        }

        private void UpdateMonitorignsList(List<WorkTime> selected)
        {
            _vm.Monitorings = selected.Select(s => new WorkTimeViewModel(s)).ToList();
        }

        public void UpdateChart()
        {
            Debug.Assert(_authenticationService.User != null);
            //todo
            _workTimes = _repository.FindAll(_authenticationService.User, null, null);
            var selected = _workTimes.Where(w => _vm.SelectedMinDate <= w.DateCreated && _vm.SelectedMaxDate >= w.EndDate).ToList();

            if (selected.Count == 0)
            {
                _vm.IsShowingStats = false;
                return;
            }
            _vm.IsShowingStats = true;

            switch (_vm.SelectedChartType)
            {
                case OverallStatsChartTypes.Applications:
                    UpdateApplicationsSeries(selected);
                    break;
                case OverallStatsChartTypes.Summary:
                    UpdateSummarySeries(selected);
                    break;
                case OverallStatsChartTypes.Monitorings:
                    UpdateMonitorignsList(selected);
                    break;
                case OverallStatsChartTypes.SingleApplication:
                    UpdateSingleApplicationSeries(selected);
                    break;
            }

        }

        private void UpdateDates()
        {
            _vm.SelectedMinDate = _startDate.AddDays(_vm.LowerDays - _vm.MinDays);
            _vm.SelectedMaxDate = _endDate.AddDays(-(_vm.MaxDays - _vm.UpperDays));
        }

        private void UpdateDateRange()
        {
            _startDate = _vm.MinDate;
            _endDate = _vm.MaxDate;
            var diff = _endDate - _startDate;
            _vm.MaxDays = (int)diff.TotalDays;
            _vm.MinDays = 0;
        }


        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OverallStatsViewModel.LowerDays):
                    UpdateDates();
                    UpdateChart();
                    break;
                case nameof(OverallStatsViewModel.UpperDays):
                    UpdateDates();
                    UpdateChart();
                    break;
                case nameof(OverallStatsViewModel.MinDate):
                    UpdateDateRange();
                    UpdateDates();
                    UpdateChart();
                    break;
                case nameof(OverallStatsViewModel.MaxDate):
                    UpdateDateRange();
                    UpdateDates();
                    UpdateChart();
                    break;
                case nameof(OverallStatsViewModel.SelectedChartType):
                    _vm.ShowAllVisibility = _vm.SelectedChartType == OverallStatsChartTypes.Applications ? Visibility.Visible : Visibility.Hidden;
                    UpdateChart();
                    break;
                case nameof(OverallStatsViewModel.SelectedExecutable):
                    UpdateChart();
                    break;
                case nameof(OverallStatsViewModel.ShowAll):
                    UpdateChart();
                    break;
            }
        }

        public void Init(OverallStatsViewModel vm)
        {
            _vm = vm;

            _vm.PropertyChanged += VmOnPropertyChanged;
            _vm.SeriesPickerViewModel.PropertyChanged += (a,b) => UpdateChart();
            UpdateChart();

            if (_workTimes.Count > 0)
            {
                SetupDateSlider();
            }
            else
            {
                _vm.IsShowingStats = false;
            }
        }
    }
}