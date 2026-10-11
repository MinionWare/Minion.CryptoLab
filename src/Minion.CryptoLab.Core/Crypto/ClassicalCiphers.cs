using System.Text;

namespace Minion.CryptoLab.Core.Crypto;

/// <summary>
/// Pre-computer-age ciphers, implemented plainly for teaching — every method here operates on
/// real text, for real, since there's nothing sensitive about toy classroom examples. Verified
/// against known textbook vectors before use (e.g. Vigenère against the standard
/// ATTACKATDAWN/LEMON -> LXFOPVEFRNHR example).
/// </summary>
public static class ClassicalCiphers
{
    /// <summary>Shifts each letter by <paramref name="shift"/> positions, wrapping around the
    /// alphabet. Negative shift decrypts. Non-letters pass through unchanged.</summary>
    public static string Caesar(string text, int shift)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c is >= 'a' and <= 'z')
                sb.Append((char)('a' + Mod(c - 'a' + shift, 26)));
            else if (c is >= 'A' and <= 'Z')
                sb.Append((char)('A' + Mod(c - 'A' + shift, 26)));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>ROT13 — Caesar with a fixed shift of 13, its own inverse.</summary>
    public static string Rot13(string text) => Caesar(text, 13);

    /// <summary>Reverses the alphabet: a&lt;-&gt;z, b&lt;-&gt;y, and so on. Its own inverse.</summary>
    public static string Atbash(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c is >= 'a' and <= 'z')
                sb.Append((char)('z' - (c - 'a')));
            else if (c is >= 'A' and <= 'Z')
                sb.Append((char)('Z' - (c - 'A')));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>Maps each letter of the alphabet to the corresponding letter of a scrambled
    /// 26-letter key (e.g. key "QWERTYUIOPASDFGHJKLZXCVBNM" maps a-&gt;Q, b-&gt;W, ...).</summary>
    public static string SimpleSubstitution(string text, string key26)
    {
        if (key26.Length != 26)
            throw new ArgumentException("Substitution key must be exactly 26 letters.", nameof(key26));

        var upperKey = key26.ToUpperInvariant();
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c is >= 'a' and <= 'z')
                sb.Append(char.ToLowerInvariant(upperKey[c - 'a']));
            else if (c is >= 'A' and <= 'Z')
                sb.Append(upperKey[c - 'A']);
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>Vigenère cipher: each letter is Caesar-shifted by the corresponding letter of a
    /// repeating keyword (A=0, B=1, ...). Set <paramref name="decrypt"/> to reverse it.</summary>
    public static string Vigenere(string text, string key, bool decrypt = false)
    {
        var upperKey = key.ToUpperInvariant();
        var sb = new StringBuilder(text.Length);
        var ki = 0;
        foreach (var c in text)
        {
            if (char.IsLetter(c))
            {
                var isLower = char.IsLower(c);
                var baseChar = isLower ? 'a' : 'A';
                var keyShift = upperKey[ki % upperKey.Length] - 'A';
                if (decrypt) keyShift = -keyShift;
                sb.Append((char)(baseChar + Mod(c - baseChar + keyShift, 26)));
                ki++;
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>Repeating-key XOR over raw bytes — the mechanism underneath the one-time pad and
    /// every stream cipher. Its own inverse: XOR-ing the output with the same key recovers the input.</summary>
    public static byte[] Xor(byte[] data, byte[] key)
    {
        var result = new byte[data.Length];
        for (var i = 0; i < data.Length; i++)
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        return result;
    }

    private static int Mod(int value, int modulus) => ((value % modulus) + modulus) % modulus;
}
