using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Triangle3DAnimationGUI.Utils
{
    public class FileSizeInfoAfterArrowConverter : IValueConverter
    {
        public static readonly FileSizeInfoAfterArrowConverter Instance = new();
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                var idx = s.IndexOf('→');
                if (idx >= 0 && idx + 1 < s.Length)
                    return s.Substring(idx + 1).Trim();
                return string.Empty;
            }
            return string.Empty;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
