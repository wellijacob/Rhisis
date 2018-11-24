using Caliburn.Micro;

namespace Rhisis.Database.Manager.ViewModels
{
    public class AboutViewModel : Screen
    {
        public void CloseWindow()
        {
            this.TryClose(true);
        }
    }
}
