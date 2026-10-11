using System.Windows;
using Minion.CryptoLab.Wpf.Services;

namespace Minion.CryptoLab.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleThemeMenuItem_Click(object sender, RoutedEventArgs e) => ThemeService.Toggle();

    private void TutorialButton_Click(object sender, RoutedEventArgs e)
    {
        var tutorial = new TutorialWindow { Owner = this };
        tutorial.ShowDialog();
    }
}
