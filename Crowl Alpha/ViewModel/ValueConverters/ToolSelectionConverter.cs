using System;
using System.Globalization;
using System.Windows.Data;

namespace Crowl_Alpha.ViewModel.ValueConverters
{
    public class ToolSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedTool = value as string;
            var toolName = parameter as string;

            // Return true if the current tool is selected, otherwise false
            return selectedTool == toolName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isChecked = (bool)value;
            var toolName = parameter as string;

            // If checked, return the tool name; otherwise return null (no selection)
            return isChecked ? toolName : null;
        }
    }
}
