using Caliburn.Micro;
using Rhisis.Database.Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Rhisis.Database.Manager
{
    public class AppBootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new SimpleContainer();

        public AppBootstrapper()
        {
            this.Initialize();
        }

        protected override void Configure()
        {
            App.Instance.ChangeLanguage("en");
            this._container.Singleton<IWindowManager, WindowManager>();
            this._container.PerRequest<MainViewModel>();
            this._container.PerRequest<AboutViewModel>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            this.DisplayRootViewFor<MainViewModel>();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
}
