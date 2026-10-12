using System;
using System.Globalization;
using System.Windows.Data;

namespace Minion.CryptoLab.Wpf.Converters;

/// <summary>A paragraph starting with "## " is rendered as a section heading instead of body
/// text — a lightweight convention so long lessons can be broken into labeled sections without a
/// model change. Strips the marker for display.</summary>
public sealed class HeadingDisplayTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && s.StartsWith("## ", StringComparison.Ordinal) ? s[3..] : value ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Companion to <see cref="HeadingDisplayTextConverter"/> — true when the paragraph is a
/// "## " section heading, used by a DataTrigger to switch styling.</summary>
public sealed class IsHeadingConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && s.StartsWith("## ", StringComparison.Ordinal);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
