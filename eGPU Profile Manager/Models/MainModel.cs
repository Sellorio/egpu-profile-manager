using eGPU_Profile_Manager.ViewModels;
using Microsoft.Win32;
using SEA.Mvvm.ModelSupport;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Input;

namespace eGPU_Profile_Manager.Models
{
    public class MainModel : ModelBase<MainViewModel>
    {
        private readonly ManagementEventWatcher _deviceAddedOrRemovedWatcher;

        public MainModel(IInterface @interface, MainViewModel viewModel)
            : base(@interface, viewModel)
        {
            Metadata.LoadInto(viewModel);

            CheckAndAutoSwitchProfile();

            _deviceAddedOrRemovedWatcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
            _deviceAddedOrRemovedWatcher.EventArrived += OnDeviceAddedOrRemoved;
            _deviceAddedOrRemovedWatcher.Start();
        }

        protected override void BindModel(ModelBinder<MainViewModel> modelBinder)
        {
            modelBinder.Bind(x => x.AddProfile, AddProfile);
            modelBinder.Bind(x => x.ManuallySelectProfile, CanManuallySelectProfile, ManuallySelectProfile);
            modelBinder.Bind(x => x.MoveProfileUp, CanMoveProfileUp, MoveProfileUp);
            modelBinder.Bind(x => x.MoveProfileDown, CanMoveProfileDown, MoveProfileDown);
            modelBinder.Bind(x => x.RemoveProfile, CanRemoveProfile, RemoveProfile);
            modelBinder.Bind(x => x.AddTrackedFile, AddTrackedFile);
            modelBinder.Bind(x => x.RemoveTrackedFile, RemoveTrackedFile);
        }

        private void AddProfile()
        {
            var newProfileInfo =
                Interface.ExclusivePopup(
                    new AddProfileViewModel { ExistingProfiles = ViewModel.Profiles });

            if (newProfileInfo == null || newProfileInfo.Name == null)
            {
                return;
            }

            var newProfile = new ProfileViewModel
            {
                Id = Guid.NewGuid(),
                Name = newProfileInfo.Name,
                DeviceName = newProfileInfo.DeviceName,
                IsProtected = false,
                IsSelected = false
            };

            ViewModel.Profiles.Insert(0, newProfile);

            var profilePath = Path.Combine(Constants.ProfilesFolder, newProfile.Id.ToString());
            Directory.CreateDirectory(profilePath);

            foreach (var file in ViewModel.TrackedFiles)
            {
                if (File.Exists(file.Path))
                {
                    File.Copy(file.Path, Path.Combine(profilePath, file.Id.ToString()));
                }
            }

            CheckAndAutoSwitchProfile();

            Metadata.Save(ViewModel);

            UpdateAllCommands();
        }

        private bool CanManuallySelectProfile(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            var profile = (ProfileViewModel)parameter;
            return !profile.IsSelected;
        }

        private void ManuallySelectProfile(object parameter)
        {
            var profile = (ProfileViewModel)parameter;
            SelectProfile(profile);

            UpdateAllCommands();
        }

        private bool CanMoveProfileUp(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            var profile = (ProfileViewModel)parameter;
            return ViewModel.Profiles.IndexOf(profile) > 0;
        }

        private void MoveProfileUp(object parameter)
        {
            var profile = (ProfileViewModel)parameter;
            int index = ViewModel.Profiles.IndexOf(profile);

            ViewModel.Profiles.Remove(profile);
            ViewModel.Profiles.Insert(index - 1, profile);

            Metadata.Save(ViewModel);

            UpdateAllCommands();
        }

        private bool CanMoveProfileDown(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            var profile = (ProfileViewModel)parameter;
            return ViewModel.Profiles.IndexOf(profile) < ViewModel.Profiles.Count - 1;
        }

        private void MoveProfileDown(object parameter)
        {
            var profile = (ProfileViewModel)parameter;
            int index = ViewModel.Profiles.IndexOf(profile);

            ViewModel.Profiles.Remove(profile);
            ViewModel.Profiles.Insert(index + 1, profile);

            Metadata.Save(ViewModel);

            UpdateAllCommands();
        }

        private bool CanRemoveProfile(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            var profile = (ProfileViewModel)parameter;
            return !profile.IsProtected;
        }

        private void RemoveProfile(object parameter)
        {
            var profile = (ProfileViewModel)parameter;

            if (profile.IsProtected)
            {
                return;
            }

            if (MessageBoxResult.Yes !=
                MessageBox.Show(
                    "Are you sure you want to delete this profile? This action cannot be reversed.",
                    "Remove Profile",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning))
            {
                return;
            }

            ViewModel.Profiles.Remove(profile);

            if (profile.IsSelected)
            {
                CheckAndAutoSwitchProfile();
            }

            Metadata.Save(ViewModel);

            UpdateAllCommands();
        }

        private void AddTrackedFile()
        {
            var fileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = true
            };

            if (fileDialog.ShowDialog() != true)
            {
                return;
            }

            foreach (var path in fileDialog.FileNames)
            {
                var fileInfo = new FileInfo(path);

                if (fileInfo.Length > 5_000_000)
                {
                    if (MessageBoxResult.Yes !=
                        MessageBox.Show(
                            fileInfo.FullName + "\r\nis quite large and may slow down the profile switching time. Are you sure you want to track it?",
                            "Quite Large File",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning))
                    {
                        continue;
                    }
                }

                var trackedFile = new TrackedFileViewModel
                {
                    Id = Guid.NewGuid(),
                    Path = path
                };

                foreach (var profile in ViewModel.Profiles)
                {
                    var profileFolder = Path.Combine(Constants.ProfilesFolder, profile.Id.ToString());
                    var profileFilePath = Path.Combine(profileFolder, trackedFile.Id.ToString());

                    Directory.CreateDirectory(profileFolder);
                    File.Copy(path, profileFilePath);
                }

                ViewModel.TrackedFiles.Add(trackedFile);
            }

            ViewModel.TrackedFiles = new ObservableCollection<TrackedFileViewModel>(ViewModel.TrackedFiles.OrderBy(x => x.Path));

            Metadata.Save(ViewModel);

            UpdateAllCommands();
        }

        private void RemoveTrackedFile(object parameter)
        {
            var file = (TrackedFileViewModel)parameter;

            if (MessageBoxResult.Yes !=
                MessageBox.Show(
                    "Are you sure you want to untrack this file? Inactive profiles will lose their data for this file.",
                    "Remove Profile",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning))
            {
                return;
            }

            foreach (var profile in ViewModel.Profiles)
            {
                var profileFilePath = Path.Combine(Constants.ProfilesFolder, profile.Id.ToString(), file.Id.ToString());

                if (File.Exists(profileFilePath))
                {
                    File.Delete(profileFilePath);
                }
            }

            ViewModel.TrackedFiles.Remove(file);

            Metadata.Save(ViewModel);

            UpdateAllCommands();
        }

        private void CheckAndAutoSwitchProfile()
        {
            var allDevices = GetAllDeviceNames();
            var newProfile =
                ViewModel.Profiles.First(x => allDevices.Any(y => x.DeviceName == null || y.Contains(x.DeviceName, StringComparison.OrdinalIgnoreCase)));

            SelectProfile(newProfile);

            UpdateAllCommands();
        }

        private void SelectProfile(ProfileViewModel profile)
        {
            if (profile != ViewModel.SelectedProfile)
            {
                if (ViewModel.SelectedProfile != null)
                {
                    var previousProfileFolder = Path.Combine(Constants.ProfilesFolder, ViewModel.SelectedProfile.Id.ToString());
                    Directory.CreateDirectory(previousProfileFolder);

                    foreach (var file in ViewModel.TrackedFiles)
                    {
                        if (File.Exists(file.Path))
                        {
                            File.Copy(file.Path, Path.Combine(previousProfileFolder, file.Id.ToString()), true);
                        }
                    }
                }

                var newProfileFolder = Path.Combine(Constants.ProfilesFolder, profile.Id.ToString());
                Directory.CreateDirectory(newProfileFolder);

                foreach (var file in ViewModel.TrackedFiles)
                {
                    var profileFilePath = Path.Combine(newProfileFolder, file.Id.ToString());

                    if (File.Exists(file.Path) && File.Exists(profileFilePath))
                    {
                        File.Copy(profileFilePath, file.Path, true);
                    }
                }

                ViewModel.SelectedProfile.IsSelected = false;
                ViewModel.SelectedProfile = profile;
                profile.IsSelected = true;
                Metadata.Save(ViewModel);
            }
        }

        private void OnDeviceAddedOrRemoved(object sender, EventArrivedEventArgs e)
        {
            _deviceAddedOrRemovedWatcher.Stop();
            // lazy debounce implementation to avoid getting spammed and hammering the profile changing logic
            System.Threading.Thread.Sleep(2000);
            _deviceAddedOrRemovedWatcher.Start();

            CheckAndAutoSwitchProfile();
        }

        private static string[] GetAllDeviceNames()
        {
            var searcher = new ManagementObjectSearcher($"SELECT Name FROM Win32_PnPEntity");
            var results = searcher.Get();

            return
                results
                    .Cast<ManagementObject>()
                    .Select(x => (string)x.Properties["Name"].Value)
                    .Where(x => x != null)
                    .ToArray();
        }

        private void UpdateAllCommands()
        {
            ViewModel.AddProfile.TriggerCanExecuteChanged();
            ViewModel.ManuallySelectProfile.TriggerCanExecuteChanged();
            ViewModel.MoveProfileUp.TriggerCanExecuteChanged();
            ViewModel.MoveProfileDown.TriggerCanExecuteChanged();
            ViewModel.RemoveProfile.TriggerCanExecuteChanged();
            ViewModel.AddTrackedFile.TriggerCanExecuteChanged();
            ViewModel.RemoveTrackedFile.TriggerCanExecuteChanged();
        }
    }
}
