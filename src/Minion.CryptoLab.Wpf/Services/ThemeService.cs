using System;
using System.Linq;
using System.Windows;
using Minion.CryptoLab.Core.Models;

namespace Minion.CryptoLab.Wpf.Services;

/// <summary>
/// Swaps the current Era.*.xaml dictionary at runtime. Every era defines the same key set
/// (Era.Background, Era.Ink, Era.Accent, ...), so every control bound via DynamicResource
/// repaints the instant the dictionary is replaced. Unlike the old Dark/Light toggle, the era
/// is driven by the current lesson's Part, not a user preference.
/// </summary>
public static class ThemeService
{
    public static Part Current { get; private set; } = Part.Hub;

    public static void Apply(Part part)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var eraDictUri = EraFile(part);

        var existing = dictionaries.FirstOrDefault(d =>
            d.Source != null && d.Source.OriginalString.Contains("Era."));

        var replacement = new ResourceDictionary { Source = new Uri(eraDictUri, UriKind.Relative) };

        if (existing != null)
        {
            int index = dictionaries.IndexOf(existing);
            dictionaries.RemoveAt(index);
            dictionaries.Insert(index, replacement);
        }
        else
        {
            dictionaries.Insert(0, replacement);
        }

        Current = part;
    }

    private static string EraFile(Part part) => part switch
    {
        Part.I => "Themes/Era.Stone.xaml",
        Part.II => "Themes/Era.Machine.xaml",
        Part.III => "Themes/Era.Dialup.xaml",
        Part.IV => "Themes/Era.Network.xaml",
        _ => "Themes/Era.Hub.xaml"
    };
}
