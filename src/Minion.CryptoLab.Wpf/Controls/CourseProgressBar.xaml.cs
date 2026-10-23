using System.Windows;
using System.Windows.Controls;

namespace Minion.CryptoLab.Wpf.Controls;

/// <summary>The header's 5-segment course progress bar with a "you are here" marker. ActualWidth
/// isn't a bindable dependency property in WPF, so the marker is repositioned explicitly on
/// SizeChanged rather than relying on the MultiBinding alone to catch window resizes.</summary>
public partial class CourseProgressBar : UserControl
{
    public static readonly DependencyProperty LessonIndexProperty =
        DependencyProperty.Register(nameof(LessonIndex), typeof(int), typeof(CourseProgressBar),
            new FrameworkPropertyMetadata(0, OnPositionChanged));

    public static readonly DependencyProperty LessonCountProperty =
        DependencyProperty.Register(nameof(LessonCount), typeof(int), typeof(CourseProgressBar),
            new FrameworkPropertyMetadata(26, OnPositionChanged));

    public int LessonIndex { get => (int)GetValue(LessonIndexProperty); set => SetValue(LessonIndexProperty, value); }
    public int LessonCount { get => (int)GetValue(LessonCountProperty); set => SetValue(LessonCountProperty, value); }

    public CourseProgressBar()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdateMarker();
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((CourseProgressBar)d).UpdateMarker();

    private void UpdateMarker()
    {
        if (ActualWidth <= 0 || LessonCount <= 0) return;
        var fraction = (LessonIndex + 0.5) / LessonCount;
        var left = System.Math.Max(0, fraction * ActualWidth - Marker.Width / 2);
        Marker.Margin = new Thickness(left, 0, 0, 0);
    }
}
