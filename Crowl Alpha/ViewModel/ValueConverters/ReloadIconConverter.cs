using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Crowl_Alpha.ViewModel.ValueConverters
{
    public class ReloadIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Ensure we have two values (Url and SearchedUrl)
            if (values.Length == 2 && values[0] is string url && values[1] is string searchedUrl)
            {
                // Check the logic to decide which icon to return
                PackIconKind iconKind;
                if (url != searchedUrl || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(searchedUrl))
                {
                    iconKind = PackIconKind.Crosshairs;
                }
                else
                {
                    iconKind = PackIconKind.Reload;
                }

                // Return a PackIcon element with the chosen PackIconKind
                return new PackIcon
                {
                    Kind = iconKind,
                    Width = 18,  // Set the desired size for the icon
                    Height = 18
                };
            }

            // Default icon if logic fails
            return new PackIcon
            {
                Kind = PackIconKind.Crosshairs,
                Width = 18,
                Height = 18
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
