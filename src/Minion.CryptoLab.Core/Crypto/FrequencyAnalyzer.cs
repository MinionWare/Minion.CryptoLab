namespace Minion.CryptoLab.Core.Crypto;

/// <summary>Counts how often each letter appears in a text — the single tool that breaks every
/// substitution-style cipher (Caesar, Atbash, simple substitution), because the shape of English
/// letter frequency survives any one-to-one letter swap.</summary>
public static class FrequencyAnalyzer
{
    /// <summary>English letter frequency (%), most common first — "ETAOIN SHRDLU" is the classic
    /// mnemonic typesetters used for exactly this reason.</summary>
    public static readonly IReadOnlyList<char> EnglishFrequencyOrder = "ETAOINSHRDLUCMFWYPGBVKJXQZ".ToCharArray();

    public static IReadOnlyList<(char Letter, int Count)> Count(string text)
    {
        var counts = new int[26];
        foreach (var c in text)
            if (char.IsLetter(c))
                counts[char.ToUpperInvariant(c) - 'A']++;

        return Enumerable.Range(0, 26)
            .Select(i => ((char)('A' + i), counts[i]))
            .OrderByDescending(x => x.Item2)
            .ToList();
    }
}
