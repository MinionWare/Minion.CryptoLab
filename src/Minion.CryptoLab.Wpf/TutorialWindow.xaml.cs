using System.Windows;
using Minion.CryptoLab.Wpf.ViewModels;

namespace Minion.CryptoLab.Wpf;

public partial class TutorialWindow : Window
{
    public TutorialWindow()
    {
        InitializeComponent();
        var viewModel = new TutorialViewModel();
        viewModel.CloseRequested += (_, _) => Close();
        DataContext = viewModel;
    }
}
