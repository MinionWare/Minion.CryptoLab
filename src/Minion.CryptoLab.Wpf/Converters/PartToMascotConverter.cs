using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Minion.CryptoLab.Core.Models;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>Same reasoning as PartTileBrushConverter: Home shows every era's mascot at once, so
/// this looks the image up directly rather than through the swappable Era.Mascot resource.</summary>
public sealed class PartToMascotConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var file = value switch
        {
            Part.I => "minion-caesar.png",
            Part.II => "minion-raw-bits.png",
            Part.III => "minion-90s-board.png",
            Part.IV => "minion-network-v4.png",
            _ => null
        };
        return file is null ? null : new BitmapImage(new Uri($"pack://application:,,,/Resources/Mascots/{file}"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
