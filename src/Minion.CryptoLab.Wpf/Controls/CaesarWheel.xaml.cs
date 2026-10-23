using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minion.CryptoLab.Wpf.Controls;

/// <summary>The Lesson 02 millstone wheel: a fixed outer ring of plain letters, an inner ring of
/// cipher letters that rotates with the shift, a hub showing the shift as a roman numeral, and a
/// stone pointer at 12 o'clock. Letters are placed once in code (WPF has no declarative circular
/// layout); only the inner ring's RotateTransform angle changes afterwards.</summary>
public partial class CaesarWheel : UserControl
{
    private const double Center = 190;
    private const double OuterRadius = 166;
    private const double InnerRadius = 116;
    private static readonly string[] RomanOnes = ["", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX"];
    private static readonly string[] RomanTens = ["", "X", "XX"];

    public static readonly DependencyProperty ShiftProperty =
        DependencyProperty.Register(nameof(Shift), typeof(int), typeof(CaesarWheel),
            new FrameworkPropertyMetadata(3, OnShiftChanged));

    public int Shift { get => (int)GetValue(ShiftProperty); set => SetValue(ShiftProperty, value); }

    public CaesarWheel()
    {
        InitializeComponent();
        Loaded += (_, _) => { BuildRing(OuterCanvas, OuterRadius, plain: true); BuildRing(InnerCanvas, InnerRadius, plain: false); UpdateShiftVisual(); };
    }

    private static void OnShiftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CaesarWheel)d).UpdateShiftVisual();

    private void UpdateShiftVisual()
    {
        if (ShiftRoman is null) return;
        ShiftRoman.Text = ToRoman(Shift);
        InnerRotate.Angle = -(Shift * 360.0 / 26.0);
    }

    private static string ToRoman(int n) => n is < 1 or > 26 ? n.ToString() : RomanTens[n / 10] + RomanOnes[n % 10];

    private static void BuildRing(Canvas canvas, double radius, bool plain)
    {
        canvas.Children.Clear();
        for (var i = 0; i < 26; i++)
        {
            var angle = i * 360.0 / 26.0 - 90; // start at 12 o'clock
            var rad = angle * Math.PI / 180;
            var x = Center + radius * Math.Cos(rad);
            var y = Center + radius * Math.Sin(rad);

            var tb = new TextBlock
            {
                Text = ((char)('A' + i)).ToString(),
                FontFamily = new FontFamily("./Resources/Fonts/#Cinzel"),
                FontWeight = FontWeights.Bold,
                FontSize = plain ? 18 : 16,
                Foreground = plain
                    ? (Brush)Application.Current.Resources["Era.Stone.CarvedText"]
                    : (Brush)Application.Current.Resources["Era.Stone.InscriptionText"]
            };
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(tb, x - tb.DesiredSize.Width / 2);
            Canvas.SetTop(tb, y - tb.DesiredSize.Height / 2);
            canvas.Children.Add(tb);
        }
    }
}
