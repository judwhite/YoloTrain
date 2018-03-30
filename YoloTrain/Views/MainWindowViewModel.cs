using System.Drawing;
using YoloTrain.Mvvm;

namespace YoloTrain.Views
{
    public interface IMainWindowViewModel : IViewModel
    {
        Bitmap CurrentImage { get; set; }
    }

    public class MainWindowViewModel : ViewModel, IMainWindowViewModel
    {
        public MainWindowViewModel()
        {
            CurrentImage = new Bitmap(Image.FromFile(@"F:\spartiates\sharks_vs_ella_3star\sharks_vs_ella_frames\scene01501.png"));
        }

        public Bitmap CurrentImage
        {
            get => Get<Bitmap>(nameof(CurrentImage));
            set => Set(nameof(CurrentImage), value);
        }
    }
}
