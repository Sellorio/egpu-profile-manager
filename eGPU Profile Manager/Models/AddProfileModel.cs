using eGPU_Profile_Manager.ViewModels;
using SEA.Mvvm.ModelSupport;
using System;
using System.Linq;
using System.Windows;

namespace eGPU_Profile_Manager.Models
{
    public class AddProfileModel : ModelBase<AddProfileViewModel>
    {
        public AddProfileModel(IInterface @interface, AddProfileViewModel viewModel)
            : base(@interface, viewModel)
        {
        }

        protected override void BindModel(ModelBinder<AddProfileViewModel> modelBinder)
        {
            modelBinder.Bind(x => x.Add, Add);
            modelBinder.Bind(x => x.Cancel, Cancel);
        }

        private void Add()
        {
            if (string.IsNullOrWhiteSpace(ViewModel.Name))
            {
                MessageBox.Show("Name is required.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(ViewModel.DeviceName))
            {
                MessageBox.Show("Device Name is required.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ViewModel.ExistingProfiles.Any(x => string.Equals(x.Name, ViewModel.Name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Name must be unique.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ViewModel.ExistingProfiles.Any(x => string.Equals(x.DeviceName, ViewModel.DeviceName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Device Name must be unique.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Interface.Close();
        }

        private void Cancel()
        {
            ViewModel.Name = null;
            ViewModel.DeviceName = null;
            Interface.Close();
        }
    }
}
