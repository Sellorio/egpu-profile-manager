using SEA.Mvvm.ViewModelSupport;
using System;
using System.Text.Json.Serialization;

namespace eGPU_Profile_Manager.ViewModels
{
    public class ProfileViewModel : ViewModel
    {
        public Guid Id
        {
            get => Get<Guid>();
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

        public bool IsProtected
        {
            get => Get<bool>();
            set => Set(value);
        }

        [JsonIgnore]
        public bool IsSelected
        {
            get => Get<bool>();
            set => Set(value);
        }
    }
}
