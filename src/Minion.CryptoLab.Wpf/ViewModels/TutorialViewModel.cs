using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Minion.CryptoLab.Core.Models;
using Minion.CryptoLab.Core.Tutorial;

namespace Minion.CryptoLab.Wpf.ViewModels;

/// <summary>Drives the opening wizard. Exactly one of three live-demo kinds is active per step
/// (plain cipher text I/O, a letter-frequency chart, or an ECB/CBC block-pattern comparison) —
/// every demo runs real cryptography on whatever you type.</summary>
public partial class TutorialViewModel : ObservableObject
{
    private readonly IReadOnlyList<TutorialStep> _steps = TutorialContent.Create();

    public TutorialViewModel()
    {
        StepTitles = _steps.Select((s, i) => $"{i + 1}. {s.Title}").ToList();
        LoadStepState();
    }

    /// <summary>Numbered lesson titles for the left-nav list — click any one to jump straight to
    /// it, independent of the linear Back/Next flow.</summary>
    public IReadOnlyList<string> StepTitles { get; }

    [ObservableProperty]
    private int stepIndex;

    public TutorialStep CurrentStep => _steps[StepIndex];
    public int StepNumber => StepIndex + 1;
    public int StepCount => _steps.Count;
    public bool CanGoBack => StepIndex > 0;
    public bool IsLastStep => StepIndex == _steps.Count - 1;
    public string NextButtonLabel => IsLastStep ? "Start exploring →" : "Next →";

    public bool HasDemo => CurrentStep.Demo != null;
    public bool HasFrequency => CurrentStep.Frequency != null;
    public bool HasBlockPattern => CurrentStep.BlockPattern != null;

    public string? DemoInputLabel => CurrentStep.Demo?.InputLabel ?? CurrentStep.Frequency?.InputLabel ?? CurrentStep.BlockPattern?.InputLabel;
    public string? DemoKeyLabel => CurrentStep.Demo?.KeyLabel;
    public bool HasDemoKey => DemoKeyLabel != null;

    [ObservableProperty]
    private string demoInput = string.Empty;

    [ObservableProperty]
    private string demoKey = string.Empty;

    public string DemoOutput => CurrentStep.Demo?.Transform(DemoInput, DemoKey) ?? string.Empty;
    public string DemoExplain => CurrentStep.Demo?.Explain(DemoInput, DemoKey) ?? string.Empty;

    public string FrequencyExplain => CurrentStep.Frequency?.Explain(DemoInput) ?? string.Empty;
    public ObservableCollection<LetterBar> FrequencyBars { get; } = new();

    public string BlockPatternExplain => CurrentStep.BlockPattern?.Explain(DemoInput) ?? string.Empty;
    public ObservableCollection<string> EcbBlocks { get; } = new();
    public ObservableCollection<string> CbcBlocks { get; } = new();

    partial void OnStepIndexChanged(int value) => LoadStepState();

    partial void OnDemoInputChanged(string value) => RecomputeDemo();
    partial void OnDemoKeyChanged(string value) => RecomputeDemo();

    private void LoadStepState()
    {
        DemoInput = CurrentStep.Demo?.DefaultInput ?? CurrentStep.Frequency?.DefaultInput ?? CurrentStep.BlockPattern?.DefaultInput ?? string.Empty;
        DemoKey = CurrentStep.Demo?.DefaultKey ?? string.Empty;

        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(StepNumber));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(NextButtonLabel));
        OnPropertyChanged(nameof(HasDemo));
        OnPropertyChanged(nameof(HasFrequency));
        OnPropertyChanged(nameof(HasBlockPattern));
        OnPropertyChanged(nameof(DemoInputLabel));
        OnPropertyChanged(nameof(DemoKeyLabel));
        OnPropertyChanged(nameof(HasDemoKey));
        RecomputeDemo();
    }

    private void RecomputeDemo()
    {
        OnPropertyChanged(nameof(DemoOutput));
        OnPropertyChanged(nameof(DemoExplain));
        OnPropertyChanged(nameof(FrequencyExplain));
        OnPropertyChanged(nameof(BlockPatternExplain));

        FrequencyBars.Clear();
        if (CurrentStep.Frequency != null)
        {
            var counts = CurrentStep.Frequency.Compute(DemoInput);
            var max = counts.Count > 0 ? Math.Max(1, counts.Max(c => c.Count)) : 1;
            foreach (var (letter, count) in counts)
                FrequencyBars.Add(new LetterBar(letter, count, count / (double)max));
        }

        EcbBlocks.Clear();
        CbcBlocks.Clear();
        if (CurrentStep.BlockPattern != null)
        {
            foreach (var b in CurrentStep.BlockPattern.ComputeEcbBlocks(DemoInput)) EcbBlocks.Add(b);
            foreach (var b in CurrentStep.BlockPattern.ComputeCbcBlocks(DemoInput)) CbcBlocks.Add(b);
        }
    }

    [RelayCommand]
    private void Back()
    {
        if (CanGoBack) StepIndex--;
    }

    [RelayCommand]
    private void Next()
    {
        if (IsLastStep) CloseRequested?.Invoke(this, EventArgs.Empty);
        else StepIndex++;
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

    public event EventHandler? CloseRequested;
}

/// <summary>One bar in the frequency-analysis chart.</summary>
public sealed record LetterBar(char Letter, int Count, double Fraction);
