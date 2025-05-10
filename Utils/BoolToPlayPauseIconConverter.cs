using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Triangle3DAnimationGUI.Utils
{
    public class BoolToPlayPauseIconConverter : IValueConverter
    {
        public static readonly BoolToPlayPauseIconConverter Instance = new();
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "⏸" : "▶";
            return "▶";
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
