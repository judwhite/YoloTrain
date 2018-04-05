using System;
using System.Globalization;
using System.Windows.Data;
using YoloTrain.Views;

namespace YoloTrain.Converters
{
    public class ImageThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            int offset = int.Parse((string)parameter);
            var viewModel = (MainWindowViewModel)value;

            int index = viewModel.CurrentImagePosition + offset - 1;
            if (viewModel.ImagePaths == null || viewModel.ImagePaths.Count <= index)
                return null;

            return viewModel.ImagePaths[index];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
