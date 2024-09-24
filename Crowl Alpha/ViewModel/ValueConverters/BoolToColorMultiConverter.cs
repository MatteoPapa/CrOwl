using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crowl_Alpha.ViewModel.ValueConverters
{
    public class BoolToColorMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Check if everything is true
            bool allTrue = true;
            foreach (var value in values)
            {
                if (value is bool boolValue && !boolValue)
                {
                    allTrue = false;
                    break;
                }
            }
            return allTrue ? Brushes.White : new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)); // 128 is the opacity
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // If you only need one-way binding, you can leave this unimplemented
        }
    }
}
