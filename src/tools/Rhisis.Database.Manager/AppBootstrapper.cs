using Caliburn.Micro;
using Rhisis.Database.Manager.ViewModels;
using System.Windows;

namespace Rhisis.Database.Manager
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            this.Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            this.DisplayRootViewFor<MainViewModel>();
        }
    }
}
