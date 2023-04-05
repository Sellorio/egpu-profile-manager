using eGPU_Profile_Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eGPU_Profile_Manager
{
    internal static class Metadata
    {
        private static readonly string _metadataPath = Path.Combine(Constants.ProfilesFolder, "metadata.json");

        internal static void Save(MainViewModel viewModel)
        {
            var data = new DataModel
            {
                ActiveProfileId = viewModel.SelectedProfile?.Id ?? default,
                Files = viewModel.TrackedFiles,
                Profiles = viewModel.Profiles
            };

            File.WriteAllText(_metadataPath, JsonSerializer.Serialize(data));
        }

        internal static void LoadInto(MainViewModel viewModel)
        {
            if (!File.Exists(_metadataPath))
            {
                viewModel.SelectedProfile = new ProfileViewModel
                {
                    Id = Guid.NewGuid(),
                    DeviceName = null,
                    IsProtected = true,
                    Name = "(default)",
                    IsSelected = true
                };

                viewModel.Profiles = new ObservableCollection<ProfileViewModel>
                {
                    viewModel.SelectedProfile
                };

                viewModel.TrackedFiles = new ObservableCollection<TrackedFileViewModel>();

                return;
            }

            var data = JsonSerializer.Deserialize<DataModel>(File.ReadAllText(_metadataPath));

            viewModel.Profiles = new ObservableCollection<ProfileViewModel>(data.Profiles);
            viewModel.TrackedFiles = new ObservableCollection<TrackedFileViewModel>(data.Files);

            viewModel.SelectedProfile =
                viewModel.Profiles.FirstOrDefault(x => x.Id == data.ActiveProfileId)
                    ?? viewModel.Profiles.FirstOrDefault(x => x.DeviceName == null);

            if (viewModel.SelectedProfile != null)
            {
                viewModel.SelectedProfile.IsSelected = true;
            }
        }

        private class DataModel
        {
            public Guid ActiveProfileId { get; set; }
            public IList<ProfileViewModel> Profiles { get; set; }
            public IList<TrackedFileViewModel> Files { get; set; }
        }
    }
}
