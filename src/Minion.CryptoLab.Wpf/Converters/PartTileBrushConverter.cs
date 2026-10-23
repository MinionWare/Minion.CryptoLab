using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Minion.CryptoLab.Core.Models;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>Home's four era tiles show all four eras' colours at once, so they can't use the
/// swappable Era.*.xaml DynamicResource system (only one era is merged in at a time). This holds
/// the same literal values from the design handoff's themes.json for that one multi-era view.
/// ConverterParameter selects which token: Background, Ink, Muted, Accent.</summary>
public sealed class PartTileBrushConverter : IValueConverter
{
    private static readonly Dictionary<Part, Dictionary<string, Color>> Palette = new()
    {
        [Part.I] = new() { ["Background"] = Color.FromRgb(0xD6, 0xCE, 0xBD), ["Ink"] = Color.FromRgb(0x3D, 0x36, 0x2C), ["Muted"] = Color.FromRgb(0x5E, 0x55, 0x46), ["Accent"] = Color.FromRgb(0xA3, 0x40, 0x2C) },
        [Part.II] = new() { ["Background"] = Color.FromRgb(0xF1, 0xE6, 0xCC), ["Ink"] = Color.FromRgb(0x2E, 0x1F, 0x12), ["Muted"] = Color.FromRgb(0x6B, 0x52, 0x38), ["Accent"] = Color.FromRgb(0xD2, 0x56, 0x1C) },
        [Part.III] = new() { ["Background"] = Color.FromRgb(0xE6, 0xDF, 0xCC), ["Ink"] = Color.FromRgb(0x24, 0x21, 0x1C), ["Muted"] = Color.FromRgb(0x5A, 0x53, 0x46), ["Accent"] = Color.FromRgb(0x2F, 0x6F, 0xB5) },
        [Part.IV] = new() { ["Background"] = Color.FromRgb(0x08, 0x33, 0x6C), ["Ink"] = Color.FromRgb(0xE8, 0xEE, 0xFC), ["Muted"] = Color.FromRgb(0xB4, 0xC3, 0xEA), ["Accent"] = Color.FromRgb(0xFF, 0xD2, 0x7A) },
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Part part && Palette.TryGetValue(part, out var tokens) && parameter is string key && tokens.TryGetValue(key, out var color))
            return new SolidColorBrush(color);
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
