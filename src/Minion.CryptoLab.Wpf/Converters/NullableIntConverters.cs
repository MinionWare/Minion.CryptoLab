using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>True when the nullable int has a value — used for the ECB "repeated block" stamp.</summary>
public sealed class NullableIntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is int;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Visible when the nullable int has a value — used for the ECB "repeated block" stamp.</summary>
public sealed class NullableIntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is int ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
