using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Triangle3DAnimationGUI.Utils
{
    public class FileSizeInfoBeforeArrowConverter : IValueConverter
    {
        public static readonly FileSizeInfoBeforeArrowConverter Instance = new();
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                var idx = s.IndexOf('→');
                if (idx >= 0)
                    return s.Substring(0, idx).TrimEnd() + " → ";
                return s;
            }
            return value;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
