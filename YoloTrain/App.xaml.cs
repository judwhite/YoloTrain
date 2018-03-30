using System;
using System.Windows;
using YoloTrain.Mvvm;
using YoloTrain.Views;

namespace YoloTrain
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Note: Known bug with App.xaml having no StartupUri and the theme speicified in XAML's Application.Resources
            Resources = (ResourceDictionary)LoadComponent(new Uri("/Themes/Default/Theme.xaml", UriKind.RelativeOrAbsolute));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = IoC.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}
