using SEA.Mvvm.Wpf;
using System.Windows;

namespace eGPU_Profile_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            WpfMvvm.Initialise();
        }
    }
}
