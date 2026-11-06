using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

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
        Loaded += (_, _) =>
        {
            var noise = BuildNoiseBrush(64, seed: 7);
            OuterNoise.Fill = noise;
            InnerNoise.Fill = noise;
            BuildRing(OuterCanvas, OuterRadius, plain: true);
            BuildRing(InnerCanvas, InnerRadius, plain: false);
            UpdateShiftVisual();
        };
    }

    private static void OnShiftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CaesarWheel)d).UpdateShiftVisual();

    private void UpdateShiftVisual()
    {
        if (ShiftRoman is null) return;
        ShiftRoman.Text = ToRoman(Shift);
        InnerRotate.Angle = -(Shift * 360.0 / 26.0);

        // The inner canvas rotates as a whole to show the shift, which would otherwise carry each
        // letter's own orientation along with it (sideways, upside-down, depending on position).
        // Counter-rotate every letter by the same amount so they stay upright on the page.
        foreach (var child in InnerCanvas.Children)
            if (child is TextBlock { RenderTransform: RotateTransform rotate }) rotate.Angle = -InnerRotate.Angle;
    }

    private static string ToRoman(int n) => n is < 1 or > 26 ? n.ToString() : RomanTens[n / 10] + RomanOnes[n % 10];

    /// <summary>Generates a tileable fractal-noise grain texture in place of the README's
    /// "260x260 tiled PNG" asset - there's no such file supplied, so this builds the same effect
    /// procedurally: a small bitmap of independently-random grayscale/alpha speckles, tiled across
    /// the ellipse via ImageBrush.TileMode. Kept subtle (narrow gray/alpha ranges, low ellipse
    /// opacity) so it reads as a faint material grain rather than visible static.</summary>
    private static ImageBrush BuildNoiseBrush(int tileSize, int seed)
    {
        var bitmap = new WriteableBitmap(tileSize, tileSize, 96, 96, PixelFormats.Pbgra32, null);
        var rnd = new Random(seed);
        var stride = tileSize * 4;
        var pixels = new byte[stride * tileSize];
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var gray = (byte)rnd.Next(90, 210);
            var alpha = (byte)rnd.Next(0, 110);
            pixels[i] = gray;
            pixels[i + 1] = gray;
            pixels[i + 2] = gray;
            pixels[i + 3] = alpha;
        }
        bitmap.WritePixels(new Int32Rect(0, 0, tileSize, tileSize), pixels, stride, 0);
        bitmap.Freeze();

        return new ImageBrush(bitmap)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, tileSize, tileSize),
            ViewportUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None
        };
    }

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
                // The single-arg FontFamily(string) ctor only resolves a "./..." pack-relative
                // reference when it's parsed from XAML, which supplies the base URI implicitly.
                // Built in code-behind with no base URI, it silently falls back to the default
                // font instead of throwing - so every letter on both rings rendered as plain
                // sans-serif. The two-arg ctor supplies the pack base URI explicitly.
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Cinzel"),
                FontWeight = FontWeights.Bold,
                FontSize = plain ? 18 : 16,
                // Era.Muted/Era.Accent (not the Stone-specific CarvedText/InscriptionText tokens):
                // those only exist while Era.Stone.xaml happens to be the merged dictionary, and
                // Loaded can fire before that's true (e.g. this control gets realized while a
                // different lesson's era is still active) - the Stone-only keys silently resolve
                // to null forever after since BuildRing only runs once. These two always resolve,
                // in every era, and happen to be the same colour in Stone anyway.
                Foreground = plain
                    ? (Brush)Application.Current.Resources["Era.Muted"]
                    : (Brush)Application.Current.Resources["Era.Accent"],
                // A flat-colored letter just sits on top of the stone; a tiny dark offset shadow
                // reads as a letter carved into it instead.
                Effect = new DropShadowEffect { Color = Colors.Black, Direction = 300, ShadowDepth = 1, BlurRadius = 1.5, Opacity = .55 }
            };
            // Inner-ring letters get a RotateTransform so UpdateShiftVisual can counter-rotate them
            // back upright whenever the ring itself turns; the outer ring never rotates, so it needs none.
            if (!plain)
            {
                tb.RenderTransformOrigin = new Point(0.5, 0.5);
                tb.RenderTransform = new RotateTransform();
            }
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(tb, x - tb.DesiredSize.Width / 2);
            Canvas.SetTop(tb, y - tb.DesiredSize.Height / 2);
            canvas.Children.Add(tb);
        }
    }
}
