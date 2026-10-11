using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>Paragraphs are flowing prose except where they embed an aligned table (Vigenère's
/// letter-by-letter breakdown) — those contain a newline and need a monospace font to actually
/// line up. Detects which is which so the XAML doesn't need per-paragraph metadata for it.</summary>
public sealed class MultilineToFontConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => Application.Current.FindResource(value is string s && s.Contains('\n') ? "Font.Code" : "Font.UI");

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
