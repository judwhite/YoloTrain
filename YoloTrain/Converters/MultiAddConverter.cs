using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace YoloTrain.Converters
{
    public class MultiAddConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return 0;

            var list = new List<object>(values);
            if (parameter != null)
                list.Add(parameter);

            int sum = 0;
            foreach (var value in list)
            {
                if (value is int)
                {
                    sum += (int)value;
                }
                else
                {
                    sum += int.Parse(value.ToString());
                }
            }

            return sum.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
