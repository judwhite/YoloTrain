using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace YoloTrain.Converters
{
    public class ImageThumbnailConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
                return null;

            var viewModel = values[0] as string[];
            if (viewModel == null)
                return null;

            int offset = (int)values[1];

            if (offset >= viewModel.Length)
                return null;

            return new BitmapImage(new Uri(viewModel[offset]));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
