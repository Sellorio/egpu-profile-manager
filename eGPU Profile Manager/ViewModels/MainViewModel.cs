using SEA.Mvvm.ViewModelSupport;
using System.Collections.ObjectModel;

namespace eGPU_Profile_Manager.ViewModels
{
    public class MainViewModel : ViewModelWithModel
    {
        public ObservableCollection<ProfileViewModel> Profiles
        {
            get => Get<ObservableCollection<ProfileViewModel>>();
            set => Set(value);
        }

        public ProfileViewModel SelectedProfile
        {
            get => Get<ProfileViewModel>();
            set => Set(value);
        }

        public ObservableCollection<TrackedFileViewModel> TrackedFiles
        {
            get => Get<ObservableCollection<TrackedFileViewModel>>();
            set => Set(value);
        }

        public CommandBase AddProfile
        {
            get => Get<CommandBase>();
            set => Set(value);
        }

        public CommandBase RemoveProfile
        {
            get => Get<CommandBase>();
            set => Set(value);
        }

        public CommandBase ManuallySelectProfile
        {
            get => Get<CommandBase>();
            set => Set(value);
        }

        public CommandBase MoveProfileUp
        {
            get => Get<CommandBase>();
            set => Set(value);
        }

        public CommandBase MoveProfileDown
        {
            get => Get<CommandBase>();
            set => Set(value);
        }

        public CommandBase AddTrackedFile
        {
            get => Get<CommandBase>();
            set => Set(value);
        }

        public CommandBase RemoveTrackedFile
        {
            get => Get<CommandBase>();
            set => Set(value);
        }
    }
}
