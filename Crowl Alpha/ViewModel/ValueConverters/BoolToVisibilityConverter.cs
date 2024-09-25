using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Crowl_Alpha.ViewModel.ValueConverters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (value is bool) && (bool)value;

            // Check if the parameter was passed, and if it's "Invert", reverse the bool value
            if (parameter != null && parameter.ToString().Equals("Invert"))
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool boolValue = (visibility == Visibility.Visible);

                // Invert the boolean value if "Invert" parameter is passed
                if (parameter != null && parameter.ToString().Equals("Invert"))
                {
                    boolValue = !boolValue;
                }

                return boolValue;
            }
            return false;
        }
    }

}
