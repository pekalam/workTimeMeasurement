using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.FaceInitialization
{
    public class FaceInitializationViewModel : BindableBase, INavigationAware
    {
        private int _progress;
        private IFaceInitializationController _controller;

        public event Action<BitmapSource> OnFrameChanged;
        public event Action<Rect> OnFaceDetected;
        public event Action OnNoFaceDetected; 
        private Brush _stepInfoPanelBrush = Brushes.Green;
        private Brush _faceRectBrush = Brushes.Blue;
        private bool _loadingOverlayVisible;
        private string _loadingOverlayText;
        private bool _stepInfoPanelVisible;
        private string _stepInfoPanelText;
        private bool _stepInfoContinueVisible;
        private bool _stepInfoRetryVisible;
        private bool _photoPreviewVisible;
        private ImageSource _photo2;
        private ImageSource _photo1;
        private ImageSource _photo3;
        private bool _faceDetected;
        private bool _stepStarted;
        private bool _instructionsVisible;
        private string _instructionsText;
        private bool _leftArrowVisible;
        private bool _rightArrowVisible;

        public FaceInitializationViewModel(IFaceInitializationController controller)
        {
            _controller = controller;
            StepInfoContinueClick = _controller.StepInfoContinueClick;
            StepInfoRetryClick = _controller.StepInfoRetryClick;
            StartFaceInitCommand = _controller.StartFaceInitCommand;
            BackCommand = _controller.BackCommand;
        }

        public ICommand StepInfoContinueClick { get; }
        public ICommand StepInfoRetryClick { get; }
        public ICommand StartFaceInitCommand { get; }
        public ICommand BackCommand { get; }

        public void CallOnFrameChanged(BitmapSource bmp)
        {
            OnFrameChanged?.Invoke(bmp);
        }

        public void CallOnFaceDetected(Rect rect)
        {
            FaceDetected = true;
            OnFaceDetected?.Invoke(rect);
        }

        public void CallOnNoFaceDetected()
        {
            FaceDetected = false;
            OnNoFaceDetected?.Invoke();
        }

        public void ShowErrorStepInfo(string msg)
        {
            StepInfoPanelBrush = Brushes.Red;
            StepInfoPanelText = msg;
            StepInfoPanelVisible = true;
            StepInfoContinueVisible = false;
            StepInfoRetryVisible = true;
        }

        public void ShowInformationStepInfo(string msg)
        {
            StepInfoPanelBrush = Brushes.Gray;
            StepInfoPanelText = msg;
            StepInfoPanelVisible = true;
            StepInfoContinueVisible = false;
            StepInfoRetryVisible = false;
        }

        public void ShowSuccessStepInfo(string msg)
        {
            StepInfoPanelBrush = Brushes.Green;
            StepInfoPanelText = msg;
            StepInfoPanelVisible = true;
            StepInfoRetryVisible = true;
            StepInfoContinueVisible = true;
        }

        public void ShowOverlay(string text)
        {
            LoadingOverlayVisible = true;
            LoadingOverlayText = text;
        }

        public void HideOverlay() => LoadingOverlayVisible = false;

        public void HideStepInfo() => StepInfoPanelVisible = false;

        public void ShowPhotoPreview(BitmapImage[] photos)
        {
            Photo1 = photos[0];
            Photo2 = photos[1];
            Photo3 = photos[2];
            PhotoPreviewVisible = true;
        }

        public void HidePhotoPreview() => PhotoPreviewVisible = false;

        public bool StepStarted
        {
            get => _stepStarted;
            set =>  SetProperty(ref _stepStarted, value);
        }

        public bool FaceDetected
        {
            get => _faceDetected;
            set =>  SetProperty(ref _faceDetected, value);
        }

        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public Brush FaceRectBrush
        {
            get => _faceRectBrush;
            set => SetProperty(ref _faceRectBrush, value);
        }

        public bool LoadingOverlayVisible
        {
            get => _loadingOverlayVisible;
            set => SetProperty(ref _loadingOverlayVisible, value);
        }

        public string LoadingOverlayText
        {
            get => _loadingOverlayText;
            set => SetProperty(ref _loadingOverlayText, value);
        }

        public Brush StepInfoPanelBrush
        {
            get => _stepInfoPanelBrush;
            set => SetProperty(ref _stepInfoPanelBrush, value);
        }

        public bool StepInfoPanelVisible
        {
            get => _stepInfoPanelVisible;
            set => SetProperty(ref _stepInfoPanelVisible, value);
        }

        public string StepInfoPanelText
        {
            get => _stepInfoPanelText;
            set => SetProperty(ref _stepInfoPanelText, value);
        }

        public bool StepInfoContinueVisible
        {
            get => _stepInfoContinueVisible;
            set => SetProperty(ref _stepInfoContinueVisible, value);
        }

        public bool StepInfoRetryVisible
        {
            get => _stepInfoRetryVisible;
            set => SetProperty(ref _stepInfoRetryVisible, value);
        }

        public bool PhotoPreviewVisible
        {
            get => _photoPreviewVisible;
            set => SetProperty(ref _photoPreviewVisible, value);
        }

        public ImageSource Photo1
        {
            get => _photo1;
            set => SetProperty(ref _photo1, value);
        }

        public ImageSource Photo2
        {
            get => _photo2;
            set => SetProperty(ref _photo2, value);
        }

        public ImageSource Photo3
        {
            get => _photo3;
            set => SetProperty(ref _photo3, value);
        }

        public bool InstructionsVisible
        {
            get => _instructionsVisible;
            set => SetProperty(ref _instructionsVisible, value);
        }

        public string InstructionsText
        {
            get => _instructionsText;
            set => SetProperty(ref _instructionsText, value);
        }

        public bool LeftArrowVisible
        {
            get => _leftArrowVisible;
            set => SetProperty(ref _leftArrowVisible, value);
        }

        public bool RightArrowVisible
        {
            get => _rightArrowVisible;
            set => SetProperty(ref _rightArrowVisible, value);
        }

        public void ShowInstructions(string text, bool leftArrow = false, bool rightArrow = false)
        {
            RightArrowVisible = rightArrow;
            LeftArrowVisible = leftArrow;
            InstructionsText = text;
            InstructionsVisible = true;
        }

        public void HideInstructions() => InstructionsVisible = false;

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await _controller.Init(this);
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
