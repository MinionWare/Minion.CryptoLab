using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Minion.CryptoLab.Core.Models;
using Minion.CryptoLab.Core.Tutorial;
using Minion.CryptoLab.Wpf.Services;

namespace Minion.CryptoLab.Wpf.ViewModels;

/// <summary>One entry in the flattened Back/Next traversal order: either a lesson or the divider
/// screen shown before the first lesson of a new part.</summary>
public abstract record NavEntry;
public sealed record DividerNavEntry(Part Part) : NavEntry;
public sealed record LessonNavEntry(int StepIndex) : NavEntry;

/// <summary>One rendered sidebar row, already resolved against the current nav position so the
/// view can bind plainly instead of comparing VM state in XAML triggers. Either a part header
/// (expanded with its lessons following, or collapsed with just a lesson count) or a lesson row.</summary>
public sealed class SidebarRowVm
{
    public required bool IsHeader { get; init; }
    public required string Label { get; init; }
    public required string Title { get; init; }
    public bool IsCurrentPartHeader { get; init; }
    public bool IsActiveLesson { get; init; }
    public int? LessonCount { get; init; }
    public int JumpStepIndex { get; init; }
}

/// <summary>Drives the tutorial: both the per-lesson demos (plain cipher text I/O, a letter-frequency
/// chart, or an ECB/CBC block-pattern comparison — every demo runs real cryptography on whatever you
/// type) and navigation across the four-era shell (sidebar, header progress bar, era divider screens,
/// the era theme itself). Back/Next walk a flattened list of lessons-and-dividers in curriculum order;
/// the sidebar can also jump straight to any lesson.</summary>
public partial class TutorialViewModel : ObservableObject
{
    private readonly IReadOnlyList<TutorialStep> _steps = TutorialContent.Create();
    private readonly IReadOnlyList<NavEntry> _nav;
    private readonly HashSet<int> _completedStepIndices = new();

    public TutorialViewModel()
    {
        _nav = BuildNav(_steps);
        LoadNavState();
    }

    private static IReadOnlyList<NavEntry> BuildNav(IReadOnlyList<TutorialStep> steps)
    {
        var nav = new List<NavEntry>();
        var lastPart = Part.Hub;
        for (var i = 0; i < steps.Count; i++)
        {
            var part = steps[i].Part;
            if (part != Part.Hub && part != lastPart) nav.Add(new DividerNavEntry(part));
            nav.Add(new LessonNavEntry(i));
            lastPart = part;
        }
        return nav;
    }

    /// <summary>The sidebar, rebuilt on every nav change: parts in curriculum order, each either
    /// expanded (current part, lessons listed) or collapsed (header + lesson count only), then the
    /// Hub-only wrap-up row.</summary>
    public ObservableCollection<SidebarRowVm> SidebarRows { get; } = new();

    private void RebuildSidebar()
    {
        SidebarRows.Clear();
        foreach (var part in new[] { Part.I, Part.II, Part.III, Part.IV })
        {
            var info = PartInfo.Of(part);
            var lessons = _steps.Select((s, i) => (StepIndex: i, Step: s)).Where(x => x.Step.Part == part).ToList();
            var isCurrent = CurrentPart == part;
            SidebarRows.Add(new SidebarRowVm
            {
                IsHeader = true, Label = info.Roman, Title = info.Title, IsCurrentPartHeader = isCurrent,
                LessonCount = isCurrent ? null : lessons.Count, JumpStepIndex = lessons[0].StepIndex
            });
            if (isCurrent)
            {
                foreach (var (stepIndex, step) in lessons)
                {
                    SidebarRows.Add(new SidebarRowVm
                    {
                        IsHeader = false, Label = step.Number, Title = step.Title,
                        IsActiveLesson = !IsDivider && stepIndex == CurrentStepIndex, JumpStepIndex = stepIndex
                    });
                }
            }
        }
        var wrapUp = _steps.Select((s, i) => (StepIndex: i, Step: s)).First(x => x.Step.Part == Part.Hub);
        SidebarRows.Add(new SidebarRowVm
        {
            IsHeader = true, Label = wrapUp.Step.Number, Title = wrapUp.Step.Title,
            IsCurrentPartHeader = CurrentPart == Part.Hub, IsActiveLesson = CurrentPart == Part.Hub && !IsDivider,
            JumpStepIndex = wrapUp.StepIndex
        });
    }

    [ObservableProperty]
    private int navIndex;

    private NavEntry CurrentNav => _nav[NavIndex];
    public bool IsDivider => CurrentNav is DividerNavEntry;
    public Part DividerPart => CurrentNav is DividerNavEntry d ? d.Part : Part.Hub;

    /// <summary>The lesson loaded "underneath" the current nav position — for a divider, this is
    /// the part's first lesson (already preloaded, just not shown yet). 0-based.</summary>
    public int CurrentStepIndex => CurrentNav switch
    {
        LessonNavEntry l => l.StepIndex,
        DividerNavEntry => FirstLessonIndexAtOrAfter(NavIndex),
        _ => 0
    };

    private int FirstLessonIndexAtOrAfter(int navIdx)
    {
        for (var i = navIdx; i < _nav.Count; i++)
            if (_nav[i] is LessonNavEntry l) return l.StepIndex;
        return _steps.Count - 1;
    }

    public TutorialStep CurrentStep => _steps[CurrentStepIndex];
    public Part CurrentPart => IsDivider ? DividerPart : CurrentStep.Part;
    public PartInfo? CurrentPartInfo => CurrentPart == Part.Hub ? null : PartInfo.Of(CurrentPart);

    /// <summary>1-based lesson position across all 26 lessons, for "Step N of 26" — stays on the
    /// upcoming lesson's number while a divider is shown.</summary>
    public int LessonPosition => CurrentStepIndex + 1;
    public int StepCount => _steps.Count;

    public bool CanGoBack => NavIndex > 0;
    private bool IsLastNav => NavIndex == _nav.Count - 1;

    public string BackButtonLabel => NavIndex > 0 ? "← " + TitleOf(_nav[NavIndex - 1]) : "← Back";
    public string NextButtonLabel => IsDivider
        ? $"Begin Lesson {CurrentStep.Number} →"
        : IsLastNav ? "Start exploring →" : TitleOf(_nav[NavIndex + 1]) + " →";

    private string TitleOf(NavEntry entry) => entry switch
    {
        DividerNavEntry d => $"Begin Lesson {FirstLessonOf(d.Part).Number}",
        LessonNavEntry l => _steps[l.StepIndex].Title,
        _ => ""
    };

    private TutorialStep FirstLessonOf(Part part) => _steps.First(s => s.Part == part);

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

    partial void OnNavIndexChanged(int value) => LoadNavState();

    partial void OnDemoInputChanged(string value) => RecomputeDemo();
    partial void OnDemoKeyChanged(string value) => RecomputeDemo();

    private void LoadNavState()
    {
        ThemeService.Apply(CurrentPart);
        if (!IsDivider) _completedStepIndices.Add(CurrentStepIndex);

        DemoInput = CurrentStep.Demo?.DefaultInput ?? CurrentStep.Frequency?.DefaultInput ?? CurrentStep.BlockPattern?.DefaultInput ?? string.Empty;
        DemoKey = CurrentStep.Demo?.DefaultKey ?? string.Empty;

        OnPropertyChanged(nameof(IsDivider));
        OnPropertyChanged(nameof(DividerPart));
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(CurrentPart));
        OnPropertyChanged(nameof(CurrentPartInfo));
        OnPropertyChanged(nameof(LessonPosition));
        OnPropertyChanged(nameof(CurrentStepIndex));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(BackButtonLabel));
        OnPropertyChanged(nameof(NextButtonLabel));
        OnPropertyChanged(nameof(HasDemo));
        OnPropertyChanged(nameof(HasFrequency));
        OnPropertyChanged(nameof(HasBlockPattern));
        OnPropertyChanged(nameof(DemoInputLabel));
        OnPropertyChanged(nameof(DemoKeyLabel));
        OnPropertyChanged(nameof(HasDemoKey));
        RecomputeDemo();
        RebuildSidebar();
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
        if (CanGoBack) NavIndex--;
    }

    [RelayCommand]
    private void Next()
    {
        if (IsLastNav) CloseRequested?.Invoke(this, EventArgs.Empty);
        else NavIndex++;
    }

    /// <summary>Jumps straight to a lesson from the sidebar, independent of the linear Back/Next
    /// flow — skips any divider that would normally sit between the current position and it.</summary>
    [RelayCommand]
    private void JumpToLesson(int stepIndex)
    {
        var target = _nav.ToList().FindIndex(n => n is LessonNavEntry l && l.StepIndex == stepIndex);
        if (target >= 0) NavIndex = target;
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

    public event EventHandler? CloseRequested;
}

/// <summary>One bar in the frequency-analysis chart.</summary>
public sealed record LetterBar(char Letter, int Count, double Fraction);
