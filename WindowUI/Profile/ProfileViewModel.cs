using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.Profile
{
    public interface IProfileViewController
    {
        void Init(ProfileViewModel vm);
        DelegateCommand RestartInit { get; }
    }

    public class ProfileViewModel : BindableBase, INavigationAware
    {
        private readonly IProfileViewController _controller;
        private ObservableCollection<BitmapImage> _referenceImgs;
        private string _username;
        private bool _algorithmStarted;

        public ProfileViewModel(IProfileViewController controller)
        {
            _controller = controller;
            RestartInit = controller.RestartInit;
        }

        public ICommand RestartInit { get; set; }

        public ObservableCollection<BitmapImage> ReferenceImgs
        {
            get => _referenceImgs;
            set => SetProperty(ref _referenceImgs, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public bool AlgorithmStarted
        {
            get => _algorithmStarted;
            set => SetProperty(ref _algorithmStarted, value);
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