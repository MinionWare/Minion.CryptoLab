using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>Paragraphs are flowing prose except where they embed an aligned table (Vigenère's
/// letter-by-letter breakdown, the CBC block trace, etc.) — those need a monospace font so the
/// columns actually line up. A bare newline isn't enough to detect that on its own (plain prose
/// with a line break — a bullet list, a multi-beat narrative scene — also contains '\n' but reads
/// worse in Code font: denser, smaller-looking, loses normal word wrap). Real aligned tables in
/// this app are always built by padding columns with multiple spaces, so requiring BOTH a newline
/// AND a run of 2+ spaces reliably tells the two apart without per-paragraph metadata.</summary>
public sealed class MultilineToFontConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => Application.Current.FindResource(
            value is string s && s.Contains('\n') && s.Contains("  ") ? "Font.Code" : "Font.UI");

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
