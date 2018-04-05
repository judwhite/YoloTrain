using System;
using System.Globalization;
using System.Windows.Data;

namespace YoloTrain.Converters
{
    public class IsEqualToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = $"{value}" == $"{parameter}";
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
