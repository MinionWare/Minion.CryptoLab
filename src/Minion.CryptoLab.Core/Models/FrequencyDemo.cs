namespace Minion.CryptoLab.Core.Models;

/// <summary>The frequency-analysis demo: type text (or feed it ciphertext), see a live letter-
/// frequency bar chart next to the known English baseline.</summary>
public sealed class FrequencyDemo
{
    public required string InputLabel { get; init; }
    public required string DefaultInput { get; init; }
    public required Func<string, IReadOnlyList<(char Letter, int Count)>> Compute { get; init; }
    public required Func<string, string> Explain { get; init; }
}
