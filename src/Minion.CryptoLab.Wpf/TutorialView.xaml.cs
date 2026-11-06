using System.Windows.Controls;
using System.Windows.Input;
using Minion.CryptoLab.Wpf.ViewModels;

namespace Minion.CryptoLab.Wpf;

/// <summary>The tutorial screen, embedded in MainWindow instead of opened as its own Window --
/// sidebar navigation (JumpToLessonCommand) already handled moving between lessons in-place, but
/// getting here required a modal dialog you had to close to get back to Home. Raises
/// HomeRequested (logo click) so MainWindow can swap back to the Home content instead.</summary>
public partial class TutorialView : UserControl
{
    public event EventHandler? HomeRequested;

    public TutorialView()
    {
        InitializeComponent();
    }

    /// <summary>Starts a fresh tutorial session, optionally jumping straight to a lesson -- same
    /// "always start clean" behaviour as the old TutorialWindow(startAtStepIndex) constructor.</summary>
    public void Start(int? startAtStepIndex)
    {
        var viewModel = new TutorialViewModel(startAtStepIndex);
        viewModel.CloseRequested += (_, _) => HomeRequested?.Invoke(this, EventArgs.Empty);
        DataContext = viewModel;
    }

    private void Logo_Click(object sender, MouseButtonEventArgs e) => HomeRequested?.Invoke(this, EventArgs.Empty);
}
