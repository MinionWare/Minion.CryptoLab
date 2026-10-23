namespace Minion.CryptoLab.Core.Crypto;

/// <summary>Toy Diffie-Hellman key exchange at a demonstrable scale (p=23, g=5 — the exact
/// textbook example from Lesson 18's design, and far too small to be secure for real use).
/// Extracted from Step18KeyExchange's original inline implementation so both the generic text
/// demo and the bespoke diagram control use the identical math.</summary>
public static class DiffieHellman
{
    public const int P = 23;
    public const int G = 5;

    public static long ModPow(long baseValue, long exponent, long modulus)
    {
        long result = 1;
        baseValue %= modulus;
        while (exponent > 0)
        {
            if ((exponent & 1) == 1) result = result * baseValue % modulus;
            exponent >>= 1;
            baseValue = baseValue * baseValue % modulus;
        }
        return result;
    }

    /// <summary>Alice and Bob's public values and the shared secret each independently computes —
    /// callers can check AliceShared == BobShared to confirm the exchange worked.</summary>
    public static (long AlicePublic, long BobPublic, long AliceShared, long BobShared) Compute(long aliceSecret, long bobSecret)
    {
        var alicePublic = ModPow(G, aliceSecret, P);
        var bobPublic = ModPow(G, bobSecret, P);
        return (alicePublic, bobPublic, ModPow(bobPublic, aliceSecret, P), ModPow(alicePublic, bobSecret, P));
    }

    /// <summary>Eve's brute force: the smallest secret that reproduces the given public value, and
    /// how many tries it took. At this toy scale she always wins; the lesson's point is that a real
    /// 2048+-bit p makes the same search computationally impossible.</summary>
    public static (long Secret, int Tries) BruteForce(long publicValue)
    {
        for (var guess = 1; guess < P; guess++)
        {
            if (ModPow(G, guess, P) == publicValue) return (guess, guess);
        }
        return (-1, P - 1);
    }
}
