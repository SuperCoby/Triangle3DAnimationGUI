using Avalonia.Data.Converters;
using System.Globalization;

namespace Triangle3DAnimationGUI.Utils
{
    public class EmptyStringToZeroConverter : IValueConverter
    {
        public static readonly EmptyStringToZeroConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Affiche toujours avec un point comme séparateur décimal, pour garantir le même comportement sur tous les PC
            if (value == null || (value is float f && f == 0) || (value is int i && i == 0))
                return "0";
            if (value is float floatVal)
                return floatVal.ToString("0.####", CultureInfo.InvariantCulture);
            if (value is double doubleVal)
                return doubleVal.ToString("0.####", CultureInfo.InvariantCulture);
            return value.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                    return 0;
                // Accepte point ou virgule à la saisie, mais parse toujours avec le point
                var normalized = s.Replace(',', '.');
                if (float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    return (float)Math.Round(f, 4); // Arrondi à 4 chiffres après la virgule
                return 0;
            }
            return value;
        }
    }
}
