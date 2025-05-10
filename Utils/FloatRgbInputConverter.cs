using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Triangle3DAnimationGUI.Utils
{
    public class FloatRgbInputConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Affichage : float -> string (toujours avec virgule)
            if (value is float f)
                return f.ToString("0.##", CultureInfo.CurrentCulture);
            return "0";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                // Nettoie la chaîne : garde chiffres et un seul séparateur décimal (point ou virgule)
                string cleaned = "";
                bool decimalFound = false;
                char? decimalChar = null;
                foreach (char c in s)
                {
                    if (char.IsDigit(c)) cleaned += c;
                    else if ((c == ',' || c == '.') && !decimalFound)
                    {
                        cleaned += c;
                        decimalFound = true;
                        decimalChar = c;
                    }
                }
                // Utilise le séparateur invariant pour le parsing
                if (decimalChar == ',')
                    cleaned = cleaned.Replace(',', '.');
                if (float.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    // Clamp entre 0 et 1
                    return Math.Clamp(result, 0f, 1f);
                }
            }
            return 0f;
        }
    }
}
