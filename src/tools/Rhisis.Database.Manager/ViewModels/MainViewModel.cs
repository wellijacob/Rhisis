using Caliburn.Micro;

namespace Rhisis.Database.Manager.ViewModels
{
    public class MainViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private bool _isReady;
        private string _databaseStatus;

        public bool IsReady
        {
            get => this._isReady;
            set
            {
                this._isReady = value;
                this.NotifyOfPropertyChange(() => IsReady);
            }
        }

        public string DatabaseStatus
        {
            get => this._databaseStatus;
            set
            {
                this._databaseStatus = value;
                this.NotifyOfPropertyChange(() => DatabaseStatus);
            }
        }

        public MainViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
            this.IsReady = false;
            this.DatabaseStatus = "Connecting to database...";
        }

        public void ShowAboutWindow()
        {
            var aboutViewModel = new AboutViewModel();
            this._windowManager.ShowDialog(aboutViewModel);
        }

        public void ChangeLanguage(string language)
        {
            App.Instance.ChangeLanguage(language);
        }
    }
}
