using SEA.Mvvm.ViewModelSupport;
using System;

namespace eGPU_Profile_Manager.ViewModels
{
    public class TrackedFileViewModel : ViewModel
    {
        public Guid Id
        {
            get => Get<Guid>();
            set => Set(value);
        }

        public string Path
        {
            get => Get<string>();
            set => Set(value);
        }
    }
}
