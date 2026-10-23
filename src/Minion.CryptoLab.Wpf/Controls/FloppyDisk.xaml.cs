using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minion.CryptoLab.Wpf.Controls;

/// <summary>One floppy disk in Lesson 14's salting demo: a name, a salt line (or "no salt"), and a
/// hash, on a body/label colour set by whichever story it's telling (identical unsalted disks vs.
/// distinct salted ones).</summary>
public partial class FloppyDisk : UserControl
{
    public static readonly DependencyProperty PersonNameProperty =
        DependencyProperty.Register(nameof(PersonName), typeof(string), typeof(FloppyDisk), new PropertyMetadata(""));
    public static readonly DependencyProperty SaltTextProperty =
        DependencyProperty.Register(nameof(SaltText), typeof(string), typeof(FloppyDisk), new PropertyMetadata(""));
    public static readonly DependencyProperty HashTextProperty =
        DependencyProperty.Register(nameof(HashText), typeof(string), typeof(FloppyDisk), new PropertyMetadata(""));
    public static readonly DependencyProperty BodyBrushProperty =
        DependencyProperty.Register(nameof(BodyBrush), typeof(Brush), typeof(FloppyDisk), new PropertyMetadata(Brushes.DarkSlateGray));
    public static readonly DependencyProperty LabelBrushProperty =
        DependencyProperty.Register(nameof(LabelBrush), typeof(Brush), typeof(FloppyDisk), new PropertyMetadata(Brushes.Ivory));
    public static readonly DependencyProperty PenBrushProperty =
        DependencyProperty.Register(nameof(PenBrush), typeof(Brush), typeof(FloppyDisk), new PropertyMetadata(Brushes.Black));

    public string PersonName { get => (string)GetValue(PersonNameProperty); set => SetValue(PersonNameProperty, value); }
    public string SaltText { get => (string)GetValue(SaltTextProperty); set => SetValue(SaltTextProperty, value); }
    public string HashText { get => (string)GetValue(HashTextProperty); set => SetValue(HashTextProperty, value); }
    public Brush BodyBrush { get => (Brush)GetValue(BodyBrushProperty); set => SetValue(BodyBrushProperty, value); }
    public Brush LabelBrush { get => (Brush)GetValue(LabelBrushProperty); set => SetValue(LabelBrushProperty, value); }
    public Brush PenBrush { get => (Brush)GetValue(PenBrushProperty); set => SetValue(PenBrushProperty, value); }

    public FloppyDisk() => InitializeComponent();
}
