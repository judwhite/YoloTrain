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
        protected override void OnStartup(StartupEventArgs e)
        {
            IoC.Resolve<MainWindow>().Show();
        }
    }
}
