using System.Linq;
using System.Windows.Input;
using Domain.User;
using Infrastructure;
using Prism.Commands;
using Prism.Regions;
using WindowUI.FaceInitialization;
using WorkTimeAlghorithm;

namespace WindowUI.Profile
{
    public class ProfileViewController : IProfileViewController
    {
        private readonly ITestImageRepository _testImageRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRegionManager _rm;
        private ProfileViewModel _vm;

        public ProfileViewController(ITestImageRepository testImageRepository, IAuthenticationService authenticationService, IRegionManager rm)
        {
            _testImageRepository = testImageRepository;
            _authenticationService = authenticationService;
            _rm = rm;
            RestartInit = new DelegateCommand(() =>
            {
                _rm.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(FaceInitializationView));
            });
        }


        public void Init(ProfileViewModel vm)
        {
            _vm = vm;

            var refImgs = _testImageRepository.GetReferenceImages(_authenticationService.User);
            _vm.ReferenceImgs = refImgs.Select(f => f.Img.ToBitmapImage()).ToObservableCollection();
            _vm.Username = _authenticationService.User.Username;
        }

        public ICommand RestartInit { get; }
    }
}