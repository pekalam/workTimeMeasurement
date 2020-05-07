using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using Rect = OpenCvSharp.Rect;

namespace WindowUI.TriggerRecognition
{
    public class TriggerRecognitionVmDispatcherDecorator : ITriggerRecognitionViewModel
    {
        private readonly ITriggerRecognitionViewModel _vm;

        public TriggerRecognitionVmDispatcherDecorator(ITriggerRecognitionViewModel vm)
        {
            _vm = vm;
        }

        public bool? FaceRecognized
        {
            get => _vm.FaceRecognized;
            set => Application.Current.Dispatcher.Invoke(() => _vm.FaceRecognized = value);
        }

        public Visibility RecognizedOverlayVisibility
        {
            get => _vm.RecognizedOverlayVisibility;
            set => Application.Current.Dispatcher.Invoke(() => _vm.RecognizedOverlayVisibility = value);
        }

        public bool Loading
        {
            get => _vm.Loading;
            set => Application.Current.Dispatcher.Invoke(() => _vm.Loading = value);
        }

        public void ShowRecognitionFailure()
        {
            Application.Current.Dispatcher.Invoke(() => _vm.ShowRecognitionFailure());
        }

        public void ShowRecognitionSuccess()
        {
            Application.Current.Dispatcher.Invoke(() => _vm.ShowRecognitionSuccess());
        }

        public void ResetRecognition()
        {
            Application.Current.Dispatcher.Invoke(() => _vm.ResetRecognition());
        }

        public void ShowLoading()
        {
            Application.Current.Dispatcher.Invoke(() => _vm.ShowLoading());
        }

        public void HideLoading()
        {
            Application.Current.Dispatcher.Invoke(() => _vm.HideLoading());
        }

        public void CallOnFrameChanged(Mat frame)
        {
            Application.Current.Dispatcher.Invoke(() => _vm.CallOnFrameChanged(frame));
        }

        public void CallOnFaceDetected(Rect rect)
        {
            Application.Current.Dispatcher.Invoke(() => _vm.CallOnFaceDetected(rect));
        }

        public void CallOnNoFaceDetected()
        {
            Application.Current.Dispatcher.Invoke(() => _vm.CallOnNoFaceDetected());
        }
    }
}