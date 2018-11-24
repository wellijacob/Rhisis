using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Rhisis.Database.Manager
{
    public partial class App : Application
    {
        public static App Instance { get; private set; }

        public App()
        {
            Instance = this;
            this.InitializeComponent();
        }
        
        public void ChangeLanguage(string culture)
        {
            var dict = new ResourceDictionary();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            if (culture == "fr")
                dict.Source = new Uri("Resources/Translations/App.fr.xaml", UriKind.Relative);
            else
                dict.Source = new Uri("Resources/Translations/App.xaml", UriKind.Relative);

            this.Resources.MergedDictionaries.RemoveAt(this.Resources.MergedDictionaries.Count - 1);
            this.Resources.MergedDictionaries.Add(dict);

            //this.Configuration.Culture = culture;
            //ConfigurationHelper.Save(this._configurationFile, this.Configuration);
        }
    }
}
