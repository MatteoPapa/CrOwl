using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crowl_Alpha.ViewModel.ValueConverters
{
    public class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = true;
            // Return true if the value is not null and not an empty string (if it's a string)
            if (value == null)
            {
                boolValue = false;
            }
            if (value is string str)
            {
                boolValue= !string.IsNullOrEmpty(str);
            }

            // Check if the 'Invert' parameter is provided and invert the result
            if (parameter != null && parameter.ToString().Trim().Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                boolValue = !boolValue;
            }


            // Return Visibility based on the boolean value
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
