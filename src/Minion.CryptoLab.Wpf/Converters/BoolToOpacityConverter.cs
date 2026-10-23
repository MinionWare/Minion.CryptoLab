using System;
using System.Globalization;
using System.Windows.Data;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>True-&gt;1.0 (fully visible), false-&gt;0.35 (faint) — used for the Caesar mapping strip's
/// "letters actually used in the message" emphasis.</summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? 1.0 : 0.35;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
