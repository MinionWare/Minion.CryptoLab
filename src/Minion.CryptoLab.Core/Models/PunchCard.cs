namespace Minion.CryptoLab.Core.Models;

/// <summary>One 16-byte block of Lesson 11's ECB/CBC demo, rendered as a punch card: the
/// plaintext that went in (padding bytes shown as "·"), the real ciphertext as 4 groups of 8 hex
/// digits, and — for ECB only — which earlier block this one is a byte-for-byte repeat of.</summary>
public sealed record PunchCard(int BlockNumber, string PlaintextFragment, string HexGrouped, int? RepeatOfBlock);
