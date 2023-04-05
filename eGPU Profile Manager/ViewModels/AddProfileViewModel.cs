using SEA.Mvvm.ViewModelSupport;
using System.Collections.ObjectModel;

namespace eGPU_Profile_Manager.ViewModels
{
    public class AddProfileViewModel : ViewModelWithModel
    {
        public ObservableCollection<ProfileViewModel> ExistingProfiles
        {
            get => Get<ObservableCollection<ProfileViewModel>>();
            set => Set(value);
        }

        public string Name
        {
            get => Get<string>();
            set => Set(value);
        }

        public string DeviceName
        {
            get => Get<string>();
            set => Set(value);
        }

        public CommandBase Add
        {
            get => Get<CommandBase>();
            set => Set(value);
        }

        public CommandBase Cancel
        {
            get => Get<CommandBase>();
            set => Set(value);
        }
    }
}
