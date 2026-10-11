using System;
using System.Globalization;
using System.Windows.Data;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>Scales a 0.0–1.0 fraction to a pixel width for the frequency-analysis bar chart.</summary>
public sealed class FractionToWidthConverter : IValueConverter
{
    private const double MaxWidth = 260;

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is double d ? Math.Max(2, d * MaxWidth) : 2.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
