using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Domain.User;
using Infrastructure;
using Infrastructure.Repositories;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Regions;
using WindowUI.MainWindow;
using WorkTimeAlghorithm;

namespace WindowUI.FaceInitialization
{
    public interface IFaceInitializationController
    {
        Task Init(FaceInitializationViewModel vm);
        ICommand StepInfoContinueClick { get; }
        ICommand StepInfoRetryClick { get; }
        ICommand StartFaceInitCommand { get; }
        ICommand BackCommand { get; }
    }
    //todo time init 3s
    public class FaceInitializationController : IFaceInitializationController
    {
        private ICommand _userPanelNavigation;
        private FaceInitializationViewModel _vm;
        private readonly ICaptureService _captureService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly ITestImageRepository _testImageRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRegionManager _regionManager;
        private readonly InitFaceService _initFaceService;
        private bool _startStep;
        private bool _stepCompleted;
        private CancellationTokenSource _camCts;

        public FaceInitializationController(ICaptureService captureService, IHcFaceDetection faceDetection,
            InitFaceService initFaceService, ITestImageRepository testImageRepository,
            IAuthenticationService authenticationService, IRegionManager regionManager)
        {
            _captureService = captureService;
            _faceDetection = faceDetection;
            _initFaceService = initFaceService;
            _testImageRepository = testImageRepository;
            _authenticationService = authenticationService;
            _regionManager = regionManager;
            StepInfoContinueClick = new DelegateCommand(BackExecute);
            StepInfoRetryClick = new DelegateCommand(StepInfoRetryExecute);
            StartFaceInitCommand = new DelegateCommand(StartFaceInitExecute);
            BackCommand = new DelegateCommand(BackExecute);
            _initFaceService.InitFaceProgress = new Progress<InitFaceProgressArgs>(OnInitFaceProgress);
        }

        private void BackExecute()
        {
            _camCts.Cancel();
            WindowUiModuleCommands.NavigateProfile.UnregisterCommand(_userPanelNavigation);
            _regionManager.Regions[ShellRegions.MainRegion].Remove(_regionManager.Regions[ShellRegions.MainRegion].ActiveViews.First());
            _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(MainWindowView));
        }

        private void StartFaceInitExecute()
        {
            _startStep = true;
        }

        private void StepInfoRetryExecute()
        {
            _initFaceService.Reset();
            _vm.Progress = 0;
            _vm.HidePhotoPreview();
            _vm.HideStepInfo();
            _vm.StepStarted = false;
            _stepCompleted = false;
        }

        private void LockUserPanelNavigation()
        {
            _userPanelNavigation = new DelegateCommand(() => { }, () => false);
            WindowUiModuleCommands.NavigateProfile.RegisterCommand(_userPanelNavigation);
        }

        public async Task Init(FaceInitializationViewModel vm)
        {
            _vm = vm;
            _vm.ShowOverlay("Initializing camera...");

            LockUserPanelNavigation();

            await Task.Run(async () =>
            {
                await StartInitFaceStep();
            });
        }

        public ICommand StepInfoContinueClick { get; }
        public ICommand StepInfoRetryClick { get; }
        public ICommand StartFaceInitCommand { get; }
        public ICommand BackCommand { get; }

        private async Task StartInitFaceStep()
        {
            _camCts = new CancellationTokenSource();
            var camEnumerator = _captureService.CaptureFrames(_camCts.Token).GetAsyncEnumerator(_camCts.Token);
            Task? stepEndTask = null;

            while (await camEnumerator.MoveNextAsync())
            {
                using var frame = camEnumerator.Current;
                Application.Current.Dispatcher.Invoke(() => _vm.CallOnFrameChanged(frame.ToBitmapImage()));

                var rects = _faceDetection.DetectFrontalThenProfileFaces(frame);

                if (_startStep)
                {
                    if (rects.Length == 1)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.HideOverlay();
                            _vm.StepStarted = true;
                        });
                       
                        stepEndTask = await _initFaceService.InitFace(_authenticationService.User, camEnumerator,
                            _camCts.Token);

                        _startStep = false;
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => _vm.ShowOverlay("Waiting for face..."));
                    }
                }


                if (rects.Length == 1)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _vm.HideOverlay();
                        _vm.CallOnFaceDetected(rects.First());
                    });
                      
                }
                else
                {
                    if (!_stepCompleted)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.ShowOverlay("Waiting for face...");
                            _vm.CallOnNoFaceDetected();
                        });
                        
                    }
                }
            }

            if (stepEndTask != null)
            {
                await stepEndTask;
            }
        }

        private void HandleFaceProgress(InitFaceProgressArgs obj)
        {
            switch (obj.ProgressState)
            {
                case InitFaceProgress.FaceNotDetected:
                    _vm.ShowErrorStepInfo("Face not detected");
                    break;
                case InitFaceProgress.FaceNotStraight:
                    _vm.ShowInstructions("Look at front of cam");
                    break;
                case InitFaceProgress.FaceNotTurnedLeft:
                    _vm.ShowInstructions("Look in following direction", rightArrow: true);
                    break;
                case InitFaceProgress.FaceNotTurnedRight:
                    _vm.ShowInstructions("Look in following direction", leftArrow: true);
                    break;
                case InitFaceProgress.FaceRecognitionError:
                    _vm.ShowErrorStepInfo("Invalid face");
                    break;
                case InitFaceProgress.PhotosTaken:
                    _vm.HideInstructions();
                    break;
                case InitFaceProgress.Progress:
                    _vm.HideStepInfo();
                    if (obj.ProgressPercentage == 100)
                    {
                        _vm.ShowSuccessStepInfo("Profile initialized");
                        _vm.ShowPhotoPreview(_testImageRepository.GetAll(_authenticationService.User)
                            .Select(p => p.Img.ToBitmapImage())
                            .ToArray());
                        _stepCompleted = true;
                    }

                    break;
            }
        }

        private void OnInitFaceProgress(InitFaceProgressArgs obj)
        {
            Debug.WriteLine(obj.ProgressState);
            Debug.WriteLine(obj.ProgressPercentage);
            _vm.Progress = obj.ProgressPercentage;

            HandleFaceProgress(obj);

            if (obj.Frame != null)
            {
                _vm.CallOnFrameChanged(obj.Frame.ToBitmapImage());
                if (obj.FaceRect.HasValue)
                {
                    _vm.CallOnFaceDetected(obj.FaceRect.Value);
                }
                else
                {
                    _vm.CallOnNoFaceDetected();
                }
            }
        }
    }
}