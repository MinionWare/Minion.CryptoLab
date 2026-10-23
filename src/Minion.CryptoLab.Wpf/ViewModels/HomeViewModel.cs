using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Minion.CryptoLab.Core.Models;
using Minion.CryptoLab.Core.Tutorial;
using Minion.CryptoLab.Wpf.Services;

namespace Minion.CryptoLab.Wpf.ViewModels;

/// <summary>One lesson pill in the selected-era panel.</summary>
public sealed record LessonPill(string Number, string Title, int StepIndex, bool Done, bool IsNext);

/// <summary>One of the four era tiles.</summary>
public sealed partial class HomeTile : ObservableObject
{
    public required Part Part { get; init; }
    public required PartInfo Info { get; init; }
    public required int CompletedCount { get; init; }
    public required int TotalCount { get; init; }
    public required string StatusText { get; init; }
    public double ProgressFraction => TotalCount == 0 ? 0 : CompletedCount / (double)TotalCount;

    [ObservableProperty]
    private bool isSelected;
}

/// <summary>Drives the Home screen: the four era tiles, the selected-era panel beneath them, and
/// the top-bar "Continue" button — all computed from CompletionStore + the static curriculum, so
/// it reflects real progress even before the tutorial window has ever been opened this run.</summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly IReadOnlyList<TutorialStep> _steps = TutorialContent.Create();

    public HomeViewModel()
    {
        CompletionStore.Changed += Refresh;
        Refresh();
    }

    public ObservableCollection<HomeTile> Tiles { get; } = new();

    [ObservableProperty]
    private HomeTile? selectedTile;

    public ObservableCollection<LessonPill> SelectedPartLessons { get; } = new();

    [ObservableProperty]
    private string selectedPartSummary = "";

    [ObservableProperty]
    private string ctaLabel = "";

    [ObservableProperty]
    private int? ctaStepIndex;

    [ObservableProperty]
    private int completedLessonCount;

    public int TotalLessonCount => _steps.Count;

    [ObservableProperty]
    private string continueLabel = "";

    [ObservableProperty]
    private int? continueStepIndex;

    private void Refresh()
    {
        Tiles.Clear();
        foreach (var part in new[] { Part.I, Part.II, Part.III, Part.IV })
        {
            var lessons = _steps.Where(s => s.Part == part).ToList();
            var done = lessons.Count(l => CompletionStore.IsCompleted(l.Number));
            var status = done == 0
                ? $"Not started · {lessons.Count} lesson{(lessons.Count == 1 ? "" : "s")}"
                : done == lessons.Count
                    ? $"✓ Complete · {done} of {lessons.Count}"
                    : $"In progress · {done} of {lessons.Count}";
            Tiles.Add(new HomeTile
            {
                Part = part, Info = PartInfo.Of(part), CompletedCount = done, TotalCount = lessons.Count, StatusText = status
            });
        }

        CompletedLessonCount = _steps.Count(s => CompletionStore.IsCompleted(s.Number));

        var nextUnfinished = _steps.FirstOrDefault(s => s.Part != Part.Hub && !CompletionStore.IsCompleted(s.Number));
        var defaultPart = nextUnfinished?.Part ?? Part.IV;
        SelectTile(Tiles.First(t => t.Part == defaultPart));

        var nextOverall = _steps.FirstOrDefault(s => !CompletionStore.IsCompleted(s.Number)) ?? _steps[0];
        ContinueStepIndex = _steps.ToList().IndexOf(nextOverall);
        ContinueLabel = $"Continue: {nextOverall.Title} →";
    }

    [RelayCommand]
    private void SelectTile(HomeTile tile)
    {
        foreach (var t in Tiles) t.IsSelected = t == tile;
        SelectedTile = tile;

        var lessons = _steps.Where(s => s.Part == tile.Part).ToList();
        var nextIndex = lessons.FindIndex(l => !CompletionStore.IsCompleted(l.Number));

        SelectedPartLessons.Clear();
        for (var i = 0; i < lessons.Count; i++)
        {
            var step = lessons[i];
            var done = CompletionStore.IsCompleted(step.Number);
            SelectedPartLessons.Add(new LessonPill(step.Number, step.Title, _steps.ToList().IndexOf(step), done, !done && i == nextIndex));
        }

        SelectedPartSummary = $"{tile.Info.Title} · {tile.Info.Era} · {tile.CompletedCount} of {tile.TotalCount} lessons complete";

        if (nextIndex < 0)
        {
            CtaLabel = "Review this part";
            CtaStepIndex = _steps.ToList().IndexOf(lessons[0]);
        }
        else if (nextIndex == 0)
        {
            CtaLabel = $"Start with {lessons[0].Title}";
            CtaStepIndex = _steps.ToList().IndexOf(lessons[0]);
        }
        else
        {
            CtaLabel = $"Continue: {lessons[nextIndex].Title}";
            CtaStepIndex = _steps.ToList().IndexOf(lessons[nextIndex]);
        }
    }
}
