using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Triangle3DAnimationGUI.Utils
{
    public class BoolToBrushConverter : IValueConverter
    {
        public static readonly BoolToBrushConverter Instance = new();
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Brushes.Red;
            return Brushes.White;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
