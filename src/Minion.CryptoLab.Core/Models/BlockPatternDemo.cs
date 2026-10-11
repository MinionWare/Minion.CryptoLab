namespace Minion.CryptoLab.Core.Models;

/// <summary>The ECB-vs-CBC block-pattern demo: real AES output, shown as one hex block per 16
/// bytes, so identical plaintext blocks under ECB visibly produce identical ciphertext blocks —
/// and don't under CBC.</summary>
public sealed class BlockPatternDemo
{
    public required string InputLabel { get; init; }
    public required string DefaultInput { get; init; }
    public required Func<string, IReadOnlyList<string>> ComputeEcbBlocks { get; init; }
    public required Func<string, IReadOnlyList<string>> ComputeCbcBlocks { get; init; }
    public required Func<string, string> Explain { get; init; }
}
