using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EasySave.MVVM.Converters
{
    public class EnumLocalizationConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;
            
            string key = value.ToString()!;
            return Services.ResourceManager.Instance.GetString(key);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
