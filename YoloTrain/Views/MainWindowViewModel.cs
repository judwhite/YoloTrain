using System.Drawing;
using System.Windows;
using System.Windows.Input;
using YoloTrain.Mvvm;

namespace YoloTrain.Views
{
    public interface IMainWindowViewModel : IViewModel
    {
        ICommand ExitCommand { get; }
        Bitmap CurrentImage { get; set; }
    }

    public class MainWindowViewModel : ViewModel, IMainWindowViewModel
    {
        public MainWindowViewModel()
        {
            ExitCommand = new DelegateCommand(() => Application.Current.MainWindow.Close());

            CurrentImage = new Bitmap(Image.FromFile(@"F:\spartiates\sharks_vs_ella_3star\sharks_vs_ella_frames\scene01501.png"));
        }

        public ICommand ExitCommand
        {
            get => Get<ICommand>(nameof(ExitCommand));
            private set => Set(nameof(ExitCommand), value);
        }

        public Bitmap CurrentImage
        {
            get => Get<Bitmap>(nameof(CurrentImage));
            set => Set(nameof(CurrentImage), value);
        }
    }
}
