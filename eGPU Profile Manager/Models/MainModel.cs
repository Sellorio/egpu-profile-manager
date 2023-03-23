using eGPU_Profile_Manager.ViewModels;
using SEA.Mvvm.ModelSupport;
using System.Management;

namespace eGPU_Profile_Manager.Models
{
    public class MainModel : ModelBase<MainViewModel>
    {
        private readonly ManagementEventWatcher _insertWatcher = new ManagementEventWatcher();
        private readonly ManagementEventWatcher _removeWatcher = new ManagementEventWatcher();

        public MainModel(IInterface @interface, MainViewModel viewModel)
            : base(@interface, viewModel)
        {
            AddListeners();
        }

        protected override void BindModel(ModelBinder<MainViewModel> modelBinder)
        {

        }

        private void OnDeviceAddedOrRemoved(object sender, EventArrivedEventArgs e)
        {
            var allDevices = GetAllDeviceNames();
        }

        private void AddListeners()
        {
            _insertWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
            _insertWatcher.EventArrived += (s, e) =>
            {
                _insertWatcher.Stop();
                _removeWatcher.Stop();
                System.Threading.Thread.Sleep(3000); // lazy debounce implementation
                _insertWatcher.Start();
                _removeWatcher.Start();

                OnDeviceAddedOrRemoved(s, e);
            };
            _insertWatcher.Start();

            _removeWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
            _removeWatcher.EventArrived += (s, e) =>
            {
                _insertWatcher.Stop();
                _removeWatcher.Stop();
                System.Threading.Thread.Sleep(3000); // lazy debounce implementation
                _insertWatcher.Start();
                _removeWatcher.Start();

                OnDeviceAddedOrRemoved(s, e);
            };
            _removeWatcher.Start();
        }

        private string[] GetAllDeviceNames()
        {
            var searcher = new ManagementObjectSearcher($"SELECT Name FROM Win32_PnPEntity");
            var results = searcher.Get();
            return results.Cast<ManagementObject>().Select(x => (string)x.Properties["Name"].Value).ToArray();
        }
    }
}
