using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Minion.CryptoLab.Core.Models;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>Looks up the always-available Era.Recap.{Part} brush (defined once in Frame.xaml, not
/// per-era-swapped) for the Wrap-up screen's recap rows.</summary>
public sealed class PartToRecapBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Part part && Application.Current.Resources[$"Era.Recap.{part}"] is Brush brush ? brush : Brushes.Gray;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
