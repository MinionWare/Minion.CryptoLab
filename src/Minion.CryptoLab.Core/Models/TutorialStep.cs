namespace Minion.CryptoLab.Core.Models;

/// <summary>One screen of the opening wizard. At most one of the three demo types is set per step
/// — whichever fits that step's lesson.</summary>
public sealed class TutorialStep
{
    public required string Title { get; init; }
    public required IReadOnlyList<string> Paragraphs { get; init; }

    /// <summary>Which curriculum part this lesson belongs to, for era theming, the sidebar grouping,
    /// and divider insertion. Defaults to Hub for the intro and wrap-up, which sit outside any part.</summary>
    public Part Part { get; init; } = Part.Hub;

    /// <summary>The lesson number as shown in the sidebar/footer ("02", "12b", "20"), not the array index.</summary>
    public string Number { get; init; } = "";

    public CipherDemo? Demo { get; init; }
    public FrequencyDemo? Frequency { get; init; }
    public BlockPatternDemo? BlockPattern { get; init; }
}
