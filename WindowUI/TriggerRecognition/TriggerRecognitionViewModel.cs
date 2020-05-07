using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Infrastructure;
using OpenCvSharp;
using Prism.Mvvm;
using Prism.Regions;
using Rect = OpenCvSharp.Rect;

namespace WindowUI.TriggerRecognition
{
    public interface ITriggerRecognitionViewModel
    {
        bool? FaceRecognized { get; set; }
        Visibility RecognizedOverlayVisibility { get; set; }
        bool Loading { get; set; }
        void ShowRecognitionFailure();
        void ShowRecognitionSuccess();
        void ResetRecognition();
        void ShowLoading();
        void HideLoading();
        void CallOnFrameChanged(Mat frame);
        void CallOnFaceDetected(Rect rect);
        void CallOnNoFaceDetected();
    }

    public class TriggerRecognitionViewModel : BindableBase, INavigationAware, ITriggerRecognitionViewModel
    {
        private readonly ITriggerRecognitionController _controller;
        private Visibility _recognizedOverlayVisibility = Visibility.Hidden;
        private bool? _faceRecognized;
        private bool _loading;

        public event Action<BitmapSource> OnFrameChanged;
        public event Action<Rect> OnFaceDetected;
        public event Action OnNoFaceDetected;

        public TriggerRecognitionViewModel(ITriggerRecognitionController controller)
        {
            _controller = controller;
        }

        public bool? FaceRecognized
        {
            get => _faceRecognized;
            set =>  SetProperty(ref _faceRecognized, value);
        }

        public Visibility RecognizedOverlayVisibility
        {
            get => _recognizedOverlayVisibility;
            set => SetProperty(ref _recognizedOverlayVisibility, value);
        }

        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        public void ShowRecognitionFailure()
        {
            RecognizedOverlayVisibility = Visibility.Visible;
            FaceRecognized = false;
        }

        public void ShowRecognitionSuccess()
        {
            RecognizedOverlayVisibility = Visibility.Visible;
            FaceRecognized = true;
        }

        public void ResetRecognition()
        {
            RecognizedOverlayVisibility = Visibility.Hidden;
            FaceRecognized = null;
        }

        public void ShowLoading()
        {
            RecognizedOverlayVisibility = Visibility.Visible;
            Loading = true;
        }

        public void HideLoading()
        {
            RecognizedOverlayVisibility = Visibility.Hidden;
            Loading = false;
        }

        public void CallOnFrameChanged(Mat frame)
        {
            OnFrameChanged?.Invoke(frame.ToBitmapImage());
        }

        public void CallOnFaceDetected(Rect rect)
        {
            OnFaceDetected?.Invoke(rect);
        }

        public void CallOnNoFaceDetected()
        {
            OnNoFaceDetected?.Invoke();
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await _controller.Init(this, (bool) navigationContext.Parameters["WindowOpened"], navigationContext.Parameters["PreviousView"]);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }

    public interface ITriggerRecognitionController
    {
        Task Init(TriggerRecognitionViewModel vm, bool windowOpened, object previousView);
    }
}