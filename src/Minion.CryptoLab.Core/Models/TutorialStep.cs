namespace Minion.CryptoLab.Core.Models;

/// <summary>One screen of the opening wizard. At most one of the three demo types is set per step
/// — whichever fits that step's lesson.</summary>
public sealed class TutorialStep
{
    public required string Title { get; init; }
    public required IReadOnlyList<string> Paragraphs { get; init; }

    public CipherDemo? Demo { get; init; }
    public FrequencyDemo? Frequency { get; init; }
    public BlockPatternDemo? BlockPattern { get; init; }
}
