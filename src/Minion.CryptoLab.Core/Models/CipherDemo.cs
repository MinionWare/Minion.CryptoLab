namespace Minion.CryptoLab.Core.Models;

/// <summary>A simple text-in, text-out live demo: type (optionally two) inputs, see the real
/// computed output and a plain-English read of what it means. Covers most of the curriculum —
/// classical ciphers, hashing, HMAC, the toy key-exchange step, randomness comparisons.</summary>
public sealed class CipherDemo
{
    public required string InputLabel { get; init; }
    public required string DefaultInput { get; init; }

    /// <summary>Null when the demo only needs one field (e.g. a hash).</summary>
    public string? KeyLabel { get; init; }
    public string? DefaultKey { get; init; }

    public required Func<string, string, string> Transform { get; init; }
    public required Func<string, string, string> Explain { get; init; }
}
