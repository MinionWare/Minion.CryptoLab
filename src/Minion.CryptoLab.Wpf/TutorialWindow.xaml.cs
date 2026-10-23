using System.Windows;
using Minion.CryptoLab.Wpf.ViewModels;

namespace Minion.CryptoLab.Wpf;

public partial class TutorialWindow : Window
{
    public TutorialWindow(int? startAtStepIndex = null)
    {
        InitializeComponent();
        var viewModel = new TutorialViewModel(startAtStepIndex);
        viewModel.CloseRequested += (_, _) => Close();
        DataContext = viewModel;
    }
}
