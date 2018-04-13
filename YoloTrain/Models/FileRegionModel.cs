using System.Windows.Media.Imaging;
using YoloTrain.Mvvm;
using YoloTrain.Utils;

namespace YoloTrain.Models
{
    public class FileRegionModel : Model
    {
        public string FileName
        {
            get => Get<string>(nameof(FileName));
            set => Set(nameof(FileName), value);
        }

        public BitmapSource BitmapImage
        {
            get => Get<BitmapSource>(nameof(BitmapImage));
            set => Set(nameof(BitmapImage), value);
        }

        public YoloCoords YoloCoords
        {
            get => Get<YoloCoords>(nameof(YoloCoords));
            set => Set(nameof(YoloCoords), value);
        }

        public int FileLineIndex
        {
            get => Get<int>(nameof(FileLineIndex));
            set => Set(nameof(FileLineIndex), value);
        }

        public string ClassName
        {
            get => Get<string>(nameof(ClassName));
            set => Set(nameof(ClassName), value);
        }
    }
}
