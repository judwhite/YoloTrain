using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YoloTrain.Converters
{
    public class IsEqualToVisibilityConverter : IValueConverter
    {
        private static readonly IsEqualToBoolConverter _isEqualToBoolConverter = new IsEqualToBoolConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)_isEqualToBoolConverter.Convert(value, targetType, parameter, culture) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
