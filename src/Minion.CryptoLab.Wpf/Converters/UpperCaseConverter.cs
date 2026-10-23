using System;
using System.Globalization;
using System.Windows.Data;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>Renders text in upper case (the design's sidebar/header labels are styled
/// text-transform: uppercase; WPF has no XAML equivalent, so this does it in the binding).</summary>
public sealed class UpperCaseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value as string)?.ToUpperInvariant() ?? value ?? "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
