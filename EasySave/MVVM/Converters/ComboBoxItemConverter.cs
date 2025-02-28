using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace EasySave.MVVM.Converters
{
    public class ComboBoxItemConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return new ComboBoxItem { Content = strValue, Tag = strValue };
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ComboBoxItem comboBoxItem)
            {
                return comboBoxItem.Tag?.ToString();
            }
            return null;
        }
    }
}
