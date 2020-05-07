using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Domain.User;
using Infrastructure;
using Infrastructure.src;
using MahApps.Metro.Controls.Dialogs;
using OpenCvSharp;
using Prism.Commands;
using Prism.Regions;
using WindowUI.MainWindow;
using WorkTimeAlghorithm;
using WorkTimeAlghorithm.StateMachine;

namespace WindowUI.TriggerRecognition
{
    public class NullableToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rev = false;
            if (parameter != null)
            {
                rev = System.Convert.ToBoolean(parameter);
            }

            if (!(value is bool b))
            {
                return Visibility.Hidden;
            }

            return rev ? !b == true ? Visibility.Visible : Visibility.Hidden : b == true ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TriggerRecognitionController : ITriggerRecognitionController
    {
        private readonly ICaptureService _captureService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly AlghorithmFaceRecognition _faceRecognition;
        private TriggerRecognitionViewModel? _vm;
        private CancellationTokenSource? _camCts;
        private readonly WorkTimeModuleService _moduleService;
        private readonly IRegionManager _rm;

        public TriggerRecognitionController(ICaptureService captureService, AlghorithmFaceRecognition faceRecognition,
            IAuthenticationService authenticationService, WorkTimeModuleService moduleService,
            IHcFaceDetection faceDetection, IRegionManager rm)
        {
            _captureService = captureService;
            _faceRecognition = faceRecognition;
            _authenticationService = authenticationService;
            _moduleService = moduleService;
            _faceDetection = faceDetection;
            _rm = rm;
        }

        private MessageDialogResult ShowRetryDialog()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
            };
            return WindowModuleStartupService.ShellWindow.ShowModalMessageExternal("Recognition failed",
                "Try again?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);
        }

        private async Task<bool> BeginFaceRecognition(IAsyncEnumerator<Mat> camEnumerator, ITriggerRecognitionViewModel vm)
        {
            //todo progress dispatch
            DateTime? start = null;
            TimeSpan timeElapsed;

            while (await camEnumerator.MoveNextAsync())
            {
                using var frame = camEnumerator.Current;
                vm.CallOnFrameChanged(frame);
                vm.HideLoading();

                var faces = _faceDetection.DetectFrontalThenProfileFaces(frame);

                if (faces.Length > 0)
                {
                    vm.CallOnFaceDetected(faces[0]);

                    if (start == null)
                    {
                        start = DateTime.Now;
                        continue;
                    }

                    timeElapsed = DateTime.Now - start.Value;


                    if (timeElapsed.TotalSeconds >= 3.0)
                    {
                        var recogTask = _faceRecognition.RecognizeFace(_authenticationService.User, frame);
                        vm.ShowLoading();
                        var (detected, recognized) = await recogTask;
                        vm.Loading = false;
                        if (detected && recognized)
                        {
                            _moduleService.Alghorithm.SetFaceRecog();
                            _camCts.Cancel();
                        }
                        else if (!recognized)
                        {
                            vm.ShowRecognitionFailure();
                            await Task.Delay(1500);
                            vm.ResetRecognition();
                            return false;
                        }

                        start = null;
                    }
                }
                else
                {
                    vm.CallOnNoFaceDetected();
                    start = null;
                }
            }

            return true;
        }

        public async Task Init(TriggerRecognitionViewModel vm, bool windowOpened, object previousView)
        {
            _vm = vm;
            _vm.ShowLoading();

            _camCts = new CancellationTokenSource();
            _moduleService.Alghorithm.StartManualFaceRecog();

            await Task.Run(async () =>
            {
                var vmDispatch = new TriggerRecognitionVmDispatcherDecorator(_vm);
                var camEnumerator = _captureService.CaptureFrames(_camCts.Token).GetAsyncEnumerator(_camCts.Token);

                bool recognized;
                do
                {
                    recognized = await BeginFaceRecognition(camEnumerator, vmDispatch);

                } while (!recognized && ShowRetryDialog() == MessageDialogResult.Affirmative);

                if (recognized)
                {
                    vmDispatch.ShowRecognitionSuccess();
                }
            });

            
            await Task.Delay(1000);

            _rm.Regions[ShellRegions.MainRegion].RemoveActiveView();
            _rm.Regions[ShellRegions.MainRegion].Activate(previousView);

            if (!windowOpened)
            {
                WindowModuleStartupService.ShellWindow.Hide();
            }
        }
    }
}