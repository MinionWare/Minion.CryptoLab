using System.Security.Cryptography;
using System.Text;

namespace Minion.CryptoLab.Core.Crypto;

/// <summary>
/// Real, modern cryptographic primitives via .NET's own System.Security.Cryptography — nothing
/// hand-rolled here, nothing simulated. Encrypting/hashing toy classroom text is harmless, so
/// unlike Minion.InjectionLab this app can run the real thing instead of narrating what it would do.
/// </summary>
public static class ModernCrypto
{
    /// <summary>Derives a fixed-size key from an arbitrary passphrase for demo purposes only —
    /// real systems should never do this (see the Lab's "hardcoded/derived key" mistake and the
    /// KDF tutorial step for why a proper key-derivation function matters).</summary>
    public static byte[] DemoKeyFromPassphrase(string passphrase, int sizeBytes = 32)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        if (sizeBytes == hash.Length) return hash;
        var key = new byte[sizeBytes];
        for (var i = 0; i < sizeBytes; i++) key[i] = hash[i % hash.Length];
        return key;
    }

    public static readonly byte[] ZeroIv16 = new byte[16];

    public static byte[] AesEcbEncrypt(byte[] plaintext, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
    }

    public static byte[] AesCbcEncrypt(byte[] plaintext, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
    }

    public static string Md5Hex(string input) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();

    public static string Sha256Hex(string input) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();

    /// <summary>PBKDF2-HMAC-SHA256, the slow/expensive-on-purpose KDF family — 600,000 iterations
    /// is OWASP's 2023 minimum recommendation for this exact combination.</summary>
    public static string Pbkdf2Hex(string password, byte[] salt, int iterations = 600_000)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return Convert.ToHexString(pbkdf2.GetBytes(32)).ToLowerInvariant();
    }

    public static string HmacSha256Hex(string message, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message))).ToLowerInvariant();
    }

    public static byte[] RandomSalt(int sizeBytes = 16) => RandomNumberGenerator.GetBytes(sizeBytes);
}
