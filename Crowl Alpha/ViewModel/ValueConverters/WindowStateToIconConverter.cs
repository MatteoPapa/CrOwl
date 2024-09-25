using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crowl_Alpha.ViewModel.ValueConverters
{
    public class WindowStateToIconConverter : IValueConverter
    {
        // Converts WindowState to PackIconKind
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowState windowState)
            {
                switch (windowState)
                {
                    case WindowState.Normal:
                        return PackIconKind.WindowMaximize;
                    case WindowState.Maximized:
                        return PackIconKind.WindowRestore;
                    default:
                        return PackIconKind.WindowMaximize;
                }
            }
            return PackIconKind.WindowMaximize;
        }

        // Not needed for one-way binding
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
