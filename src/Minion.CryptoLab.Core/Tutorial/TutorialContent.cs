using System.Diagnostics;
using Minion.CryptoLab.Core.Crypto;
using Minion.CryptoLab.Core.Models;

namespace Minion.CryptoLab.Core.Tutorial;

/// <summary>
/// The opening wizard: a from-scratch, slow-build progression from "decoder ring" substitution
/// ciphers all the way to modern authenticated encryption, hashing, KDFs, and key exchange. Every
/// demo runs REAL cryptography on whatever you type — unlike Minion.InjectionLab, there's nothing
/// unsafe about encrypting or hashing toy classroom text, so nothing here is simulated or narrated.
/// </summary>
public static class TutorialContent
{
    public static IReadOnlyList<TutorialStep> Create() => new[]
    {
        Step01Intro(),
        Step02Caesar(),
        Step03Rot13Atbash(),
        Step04SimpleSubstitution(),
        Step05FrequencyAnalysis(),
        Step06Vigenere(),
        Step07Kasiski(),
        Step08XorAndOtp(),
        Step09TwoTimePad(),
        Step10BlockVsStream(),
        Step11EcbVsCbc(),
        Step12AuthenticatedEncryption(),
        Step13HashingVsEncryption(),
        Step14Salting(),
        Step15SlowKdf(),
        Step16Hmac(),
        Step17PaddingOracle(),
        Step18KeyExchange(),
        Step19Randomness(),
        Step20WrapUp()
    };

    // ---------------------------------------------------------------- 1. Intro

    private static TutorialStep Step01Intro() => new()
    {
        Title = "What is a cipher, really?",
        Paragraphs = new[]
        {
            "A cipher is just a rule for turning a message into something else, and back again, " +
            "using a secret called a key. That's the entire idea — everything in this tutorial, " +
            "from a decoder ring to the encryption protecting your bank's website, is a variation " +
            "on \"a rule, plus a secret, applied to a message.\"",
            "The oldest ciphers used rules simple enough to do by hand: shift every letter, swap " +
            "every letter for another, that kind of thing. We'll start there — not because anyone " +
            "should use them today, but because the exact reasons they FAIL teach you what a strong " +
            "cipher actually has to get right. Every lesson here builds on the one before it.",
            "One promise up front: every demo in this app runs real, working cryptography on " +
            "whatever you type. Nothing is faked or narrated — encrypting or hashing a practice " +
            "sentence is completely harmless, so there's no reason to simulate it."
        }
    };

    // ---------------------------------------------------------------- 2. Caesar

    private static TutorialStep Step02Caesar() => new()
    {
        Title = "The decoder ring: the Caesar cipher",
        Paragraphs = new[]
        {
            "Julius Caesar reportedly shifted every letter in his military messages forward by a " +
            "fixed amount — A becomes D, B becomes E, and so on, wrapping back to A after Z. The " +
            "\"key\" is just that one number. This is exactly what a toy decoder ring does.",
            "To decrypt, the receiver shifts backward by the same amount. Try changing the shift " +
            "amount below and watch the same message turn into completely different-looking text — " +
            "but the SAME shift always produces the SAME output, every time. That predictability is " +
            "the whole mechanism, and (spoiler) also exactly why it's breakable."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message",
            DefaultInput = "Attack at dawn",
            KeyLabel = "Shift amount (the key)",
            DefaultKey = "3",
            Transform = (input, key) =>
            {
                var shift = int.TryParse(key, out var s) ? s : 3;
                return ClassicalCiphers.Caesar(input, shift);
            },
            Explain = (input, key) =>
            {
                var shift = int.TryParse(key, out var s) ? s : 3;
                return $"Every letter moved forward by {shift} position(s), wrapping Z back to A. " +
                       $"There are only 25 possible shifts (26 minus \"shift by nothing\") — a human " +
                       "can try every single one in under a minute, no computer required. That's the " +
                       "entire keyspace, and it's laughably small by modern standards.";
            }
        }
    };

    // ---------------------------------------------------------------- 3. ROT13 / Atbash

    private static TutorialStep Step03Rot13Atbash() => new()
    {
        Title = "Two famous special cases",
        Paragraphs = new[]
        {
            "ROT13 is just a Caesar shift of exactly 13 — and 13 is half of 26, so applying it " +
            "TWICE gets you back to the original text. That makes it its own inverse: the same " +
            "operation both encodes and decodes. It's still used today on forums to hide spoilers " +
            "or puzzle answers from casual glances — it was never meant to be secure.",
            "Atbash is older (it appears in the Hebrew Bible) and works differently: instead of " +
            "shifting, it reverses the whole alphabet — A swaps with Z, B with Y, and so on. Like " +
            "ROT13, applying it twice returns the original. Neither one has a \"key\" you can change " +
            "at all — the rule itself is the entire secret, which is an even weaker design than " +
            "Caesar's adjustable shift."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message",
            DefaultInput = "Meet me at midnight",
            Transform = (input, _) => $"ROT13:  {ClassicalCiphers.Rot13(input)}\nAtbash: {ClassicalCiphers.Atbash(input)}",
            Explain = (input, _) =>
                "Try pasting the ROT13 output back into the box above — you'll get your original " +
                "message back. Same for Atbash. Neither has a key to guess because there's nothing " +
                "to guess: once you know it's ROT13 or Atbash, you already know everything."
        }
    };

    // ---------------------------------------------------------------- 4. Simple substitution

    private static TutorialStep Step04SimpleSubstitution() => new()
    {
        Title = "A much bigger keyspace: simple substitution",
        Paragraphs = new[]
        {
            "Why stop at shifting the alphabet? A simple substitution cipher scrambles it into any " +
            "order at all, and maps each real letter to whatever letter sits in that position. " +
            "Caesar's shift is really just one very narrow kind of substitution — this is the " +
            "general version.",
            "Here's the number that should sound reassuring: there are 26! (26 factorial) possible " +
            "scrambled alphabets — that's roughly 400,000,000,000,000,000,000,000,000 possible " +
            "keys. Trying every single one, even at a billion attempts per second, would take far " +
            "longer than the universe has existed. Surely THIS is unbreakable?",
            "It isn't — and the next lesson is exactly why. Keyspace size only matters if the " +
            "ONLY way to attack a cipher is guessing the whole key at once."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message",
            KeyLabel = "Scrambled alphabet (26 letters, a-z in some order)",
            DefaultInput = "This looks impossible to crack",
            DefaultKey = "QWERTYUIOPASDFGHJKLZXCVBNM",
            Transform = (input, key) => key.Length == 26 ? ClassicalCiphers.SimpleSubstitution(input, key) : "Key must be exactly 26 letters.",
            Explain = (input, key) =>
                "Each letter a-z was swapped for the letter in the matching position of your " +
                "scrambled alphabet. With 26! possible keys, brute-forcing every arrangement is " +
                "completely impractical — which is exactly why real attacks on this cipher never " +
                "try to guess the key directly at all."
        }
    };

    // ---------------------------------------------------------------- 5. Frequency analysis

    private static TutorialStep Step05FrequencyAnalysis() => new()
    {
        Title = "The attack that doesn't care about keyspace: frequency analysis",
        Paragraphs = new[]
        {
            "In ordinary English text, the letter E shows up far more often than any other letter — " +
            "about 1 in every 8 letters. T, A, O, I, N follow close behind (typesetters memorized " +
            "this as \"ETAOIN SHRDLU\"). A simple substitution cipher swaps letters one-for-one, so " +
            "it can hide WHICH letter means \"E\" — but it can't hide the fact that SOME letter in " +
            "the ciphertext is going to show up roughly 1-in-8 times, because that letter is " +
            "standing in for E on every single occurrence.",
            "Count the letters in a long enough ciphertext, find the most common one, guess it's " +
            "probably E, find the second-most common, guess T — and the 26!-sized keyspace " +
            "becomes irrelevant. You never tried a single key. You read the shape of the language " +
            "straight through the disguise.",
            "This is the single most important lesson in this entire tutorial: a cipher's strength " +
            "isn't just \"how many keys are there\" — it's whether the output leaks information " +
            "about the input at all. Every lesson from here on is really about that same question, " +
            "asked of newer and newer ciphers."
        },
        Frequency = new FrequencyDemo
        {
            InputLabel = "Paste some text (or a substitution-ciphertext) to analyze",
            DefaultInput = "Guvf ybbxf vzcbffvoyr gb penpx ohg vg vfa'g vg whfg frrzf gung jnl",
            Compute = text => FrequencyAnalyzer.Count(text),
            Explain = text =>
            {
                var counts = FrequencyAnalyzer.Count(text);
                var top = counts.FirstOrDefault(c => c.Count > 0);
                return top.Count > 0
                    ? $"The most common letter here is '{top.Letter}', appearing {top.Count} time(s). " +
                      "In real English, that spot almost always belongs to E. If this is a substitution " +
                      $"ciphertext, '{top.Letter}' is a strong first guess for whatever letter maps to E."
                    : "Type or paste some letters to see their frequency.";
            }
        }
    };

    // ---------------------------------------------------------------- 6. Vigenère

    private static TutorialStep Step06Vigenere() => new()
    {
        Title = "Fixing the frequency leak (sort of): the Vigenère cipher",
        Paragraphs = new[]
        {
            "If a FIXED substitution leaks frequency, what if the substitution changed every " +
            "letter? The Vigenère cipher (1553) does exactly that: a repeating keyword picks a " +
            "different Caesar shift for each letter in turn. Key \"LEMON\" means: shift the 1st " +
            "letter by L (11), the 2nd by E (4), the 3rd by M (12), and so on, looping the keyword " +
            "for as long as the message runs.",
            "This single idea defeated simple frequency analysis for over 300 years and earned the " +
            "nickname \"le chiffre indéchiffrable\" — the indecipherable cipher. The same plaintext " +
            "letter now produces different ciphertext letters depending on where it falls relative " +
            "to the key, which smears out the frequency signature that broke substitution."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message",
            KeyLabel = "Keyword",
            DefaultInput = "ATTACKATDAWN",
            DefaultKey = "LEMON",
            Transform = (input, key) => string.IsNullOrEmpty(key) ? "Enter a keyword." : ClassicalCiphers.Vigenere(input, key),
            Explain = (input, key) =>
                $"Each letter was shifted by a different amount depending on its position and the " +
                $"repeating keyword \"{key.ToUpperInvariant()}\" — the letter 'A' doesn't always " +
                "become the same ciphertext letter anymore, unlike Caesar or simple substitution."
        }
    };

    // ---------------------------------------------------------------- 7. Kasiski

    private static TutorialStep Step07Kasiski() => new()
    {
        Title = "...but the key still repeats: Kasiski examination",
        Paragraphs = new[]
        {
            "Vigenère's fatal flaw: the keyword REPEATS. If the key is 5 letters long and two " +
            "identical 3-letter chunks of plaintext happen to land at positions that are a multiple " +
            "of 5 apart, they'll encrypt to the exact same ciphertext letters both times — because " +
            "the same key letters lined up with them both times.",
            "Friedrich Kasiski's 1863 method: scan the ciphertext for repeated sequences, measure " +
            "the distance between each repeat, and find what those distances have in common. That " +
            "shared factor is almost always the key's length. Once you know the key is, say, 5 " +
            "letters long, the message splits into 5 separate buckets — and each bucket was shifted " +
            "by the SAME single letter the whole way through, which means each bucket is just a " +
            "plain Caesar cipher again, breakable by the frequency analysis from two lessons ago.",
            "This is a recurring shape in this field: a clever idea (repeating key) solves one " +
            "problem (fixed substitution) but reintroduces a smaller version of the same problem " +
            "(repetition) somewhere else."
        },
        Demo = new CipherDemo
        {
            InputLabel = "A longer message (needs some length for coincidental repeats to appear)",
            KeyLabel = "Keyword",
            DefaultInput = "THE ENEMY WILL ATTACK AT DAWN FROM THE SOUTH THE ENEMY WILL ATTACK AGAIN BY NOON TOMORROW",
            DefaultKey = "FOX",
            Transform = (input, key) => string.IsNullOrEmpty(key) ? "Enter a keyword." : ClassicalCiphers.Vigenere(input, key),
            Explain = (input, key) => KasiskiExplain(input, key)
        }
    };

    private static string KasiskiExplain(string plaintext, string key)
    {
        if (string.IsNullOrEmpty(key)) return "Enter a keyword to encrypt the message first.";
        var ciphertext = ClassicalCiphers.Vigenere(plaintext, key);
        var lettersOnly = new string(ciphertext.Where(char.IsLetter).ToArray()).ToUpperInvariant();

        var positions = new Dictionary<string, List<int>>();
        for (var i = 0; i <= lettersOnly.Length - 3; i++)
        {
            var trigram = lettersOnly.Substring(i, 3);
            if (!positions.TryGetValue(trigram, out var list))
                positions[trigram] = list = new List<int>();
            list.Add(i);
        }

        var repeats = positions.Where(p => p.Value.Count > 1).Take(4).ToList();
        if (repeats.Count == 0)
            return "No repeated 3-letter sequences found in this ciphertext yet — try a longer " +
                   "message. Kasiski's method needs enough text for coincidental repeats to appear.";

        var lines = repeats.Select(r =>
        {
            var distances = r.Value.Skip(1).Select(o => o - r.Value[0]);
            return $"\"{r.Key}\" repeats at position(s) {string.Join(", ", r.Value)} — distance: {string.Join(", ", distances)}";
        });

        return "Repeated 3-letter sequences found in the ciphertext:\n" + string.Join("\n", lines) +
               $"\n\nYour real key here is {key.Length} letters long. Notice how the distances above " +
               "tend to share that as a common factor — that's not a coincidence, and it's exactly " +
               "what a real Kasiski examination would use to recover the key length without ever " +
               "seeing the key itself.";
    }

    // ---------------------------------------------------------------- 8. XOR / OTP

    private static TutorialStep Step08XorAndOtp() => new()
    {
        Title = "The bit-level version: XOR and the one-time pad",
        Paragraphs = new[]
        {
            "Computers don't see letters — they see bits. XOR (\"exclusive or\") is a simple bit " +
            "operation: 0⊕0=0, 1⊕1=0, 0⊕1=1, 1⊕0=1 — it's 1 exactly when the two inputs differ. " +
            "XOR-ing data with a repeating key, one byte at a time, is the exact same idea as " +
            "Vigenère, just operating on bits instead of letters. And XOR has a wonderful property: " +
            "XOR-ing the output with the same key again gets you back the original — the same " +
            "operation encrypts AND decrypts.",
            "Now take that idea to its logical extreme: what if the \"key\" is exactly as long as " +
            "the message, made of truly random bits, and used exactly once, ever? That's the " +
            "one-time pad — and it is mathematically, provably unbreakable. Not \"unbreakable with " +
            "today's computers\" — unbreakable, full stop, even with infinite computing power, " +
            "because every possible plaintext is equally consistent with the ciphertext.",
            "That's a remarkable guarantee — with three conditions that are brutally hard to " +
            "satisfy in practice: the key must be truly random (not computer-generated " +
            "pseudo-randomness), it must be as long as the entire message, and it must NEVER be " +
            "reused for a second message. Break any one of those three rules and the guarantee " +
            "evaporates completely — which is exactly the next lesson."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message",
            KeyLabel = "Key (repeats if shorter than the message)",
            DefaultInput = "attack at dawn",
            DefaultKey = "k7Qp",
            Transform = (input, key) =>
            {
                if (string.IsNullOrEmpty(key)) return "Enter a key.";
                var data = System.Text.Encoding.UTF8.GetBytes(input);
                var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
                return Convert.ToHexString(ClassicalCiphers.Xor(data, keyBytes)).ToLowerInvariant();
            },
            Explain = (input, key) =>
                "This is raw bytes now, shown as hex, not letters — XOR doesn't respect the " +
                "alphabet the way Caesar/Vigenère do, it just flips bits. If this key were truly " +
                "random, exactly as long as the message, and used only this once, this ciphertext " +
                "would be a genuine one-time pad: provably unbreakable."
        }
    };

    // ---------------------------------------------------------------- 9. Two-time pad

    private static readonly byte[] TwoTimePadDemoKey = System.Text.Encoding.UTF8.GetBytes("X7qP2mZ9vL4sN1wK");

    private static TutorialStep Step09TwoTimePad() => new()
    {
        Title = "Break rule #3 once and it all falls apart: the two-time pad",
        Paragraphs = new[]
        {
            "\"Never reuse the key\" sounds like a small operational detail. It's not — it's load-" +
            "bearing. Encrypt two DIFFERENT messages with the SAME key, and an attacker who only " +
            "sees both ciphertexts (never the key, never the plaintexts) can XOR the two " +
            "ciphertexts together. Watch what happens to the key:",
            "C1 ⊕ C2 = (P1 ⊕ K) ⊕ (P2 ⊕ K) = P1 ⊕ P2 ⊕ (K ⊕ K) = P1 ⊕ P2 — the key cancels out " +
            "completely, XOR-ed with itself, leaving the XOR of the two original messages. That's " +
            "not the plaintext yet, but it's no longer random-looking noise either — it's " +
            "structured data an attacker can prise apart using the same letter-frequency thinking " +
            "from Lesson 5, a technique called \"crib-dragging\" (guessing a likely word in one " +
            "message and sliding it along to see what it reveals in the other).",
            "This single mistake — key reuse — is such a classic, catastrophic failure that it has " +
            "sunk real cryptographic systems historically (the Soviet VENONA project was partly " +
            "broken this exact way). It's also the conceptual ancestor of a very real, very modern " +
            "mistake you'll see in the Lab: reusing a nonce/IV with a stream cipher like AES-CTR or " +
            "AES-GCM is the identical bug wearing different clothes."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message 1",
            KeyLabel = "Message 2 (both will be \"encrypted\" with the SAME hidden key)",
            DefaultInput = "MEET AT THE OLD BRIDGE",
            DefaultKey = "SEND MORE TROOPS NOW",
            Transform = (p1, p2) =>
            {
                var b1 = System.Text.Encoding.UTF8.GetBytes(p1);
                var b2 = System.Text.Encoding.UTF8.GetBytes(p2);
                var len = Math.Min(b1.Length, b2.Length);
                var c1 = ClassicalCiphers.Xor(b1[..len], TwoTimePadDemoKey);
                var c2 = ClassicalCiphers.Xor(b2[..len], TwoTimePadDemoKey);
                var xorOfCiphertexts = ClassicalCiphers.Xor(c1, c2);
                var xorOfPlaintexts = ClassicalCiphers.Xor(b1[..len], b2[..len]);
                return $"C1 (hex):              {Convert.ToHexString(c1).ToLowerInvariant()}\n" +
                       $"C2 (hex):              {Convert.ToHexString(c2).ToLowerInvariant()}\n" +
                       $"C1 XOR C2:             {Convert.ToHexString(xorOfCiphertexts).ToLowerInvariant()}\n" +
                       $"P1 XOR P2 (for proof): {Convert.ToHexString(xorOfPlaintexts).ToLowerInvariant()}\n" +
                       $"Identical? {(Convert.ToHexString(xorOfCiphertexts) == Convert.ToHexString(xorOfPlaintexts) ? "YES — the key is completely gone" : "no")}";
            },
            Explain = (p1, p2) =>
                "Notice the attacker never needed the key at all — just two ciphertexts produced " +
                "with the same one. \"C1 XOR C2\" and \"P1 XOR P2\" match exactly. Recovering the " +
                "two readable messages from that combined value takes more work (crib-dragging, " +
                "guessing likely words), but the key's protection is already completely gone the " +
                "moment it was reused."
        }
    };

    // ---------------------------------------------------------------- 10. Block vs stream / meet AES

    private static TutorialStep Step10BlockVsStream() => new()
    {
        Title = "Modern ciphers: block vs. stream, and meet AES",
        Paragraphs = new[]
        {
            "Modern symmetric ciphers (same key encrypts and decrypts, like everything so far) " +
            "come in two shapes. A STREAM cipher generates a long pseudo-random keystream from the " +
            "key and XORs it with the data, one bit/byte at a time — conceptually a one-time pad " +
            "where the \"random\" key comes from a formula instead of true randomness. A BLOCK " +
            "cipher instead encrypts fixed-size chunks (\"blocks\") of data at a time, scrambling " +
            "each block thoroughly using the key.",
            "AES (the Advanced Encryption Standard) is the block cipher essentially the entire " +
            "world standardized on after a public, multi-year competition in the late 1990s. It " +
            "operates on 16-byte (128-bit) blocks, with keys of 128, 192, or 256 bits. We're not " +
            "going to derive its internals here (it involves a structured sequence of substitution " +
            "and permutation rounds) — what matters for this tutorial is a single, sharp question: " +
            "a block cipher only tells you how to scramble ONE 16-byte block. What do you do when " +
            "your real message is longer than 16 bytes?",
            "The answer is called a \"mode of operation,\" and getting it wrong is one of the most " +
            "common real-world encryption mistakes that exists — common enough that it gets its own " +
            "lesson next, and its own entries in the Lab."
        }
    };

    // ---------------------------------------------------------------- 11. ECB vs CBC

    private static TutorialStep Step11EcbVsCbc() => new()
    {
        Title = "Modes of operation: ECB vs. CBC",
        Paragraphs = new[]
        {
            "The simplest possible mode is ECB (Electronic Codebook): split the message into " +
            "16-byte blocks and encrypt each one independently with the same key. Simple — and " +
            "exactly as flawed as simple substitution was. Identical plaintext blocks ALWAYS " +
            "produce identical ciphertext blocks, because nothing about the encryption depends on " +
            "where the block sits in the message. Patterns in the input leak straight through as " +
            "patterns in the output — this is the infamous \"ECB penguin\" effect, where encrypting " +
            "an image with large flat-colored areas with ECB leaves the outline of the original " +
            "image clearly visible in the \"encrypted\" version.",
            "CBC (Cipher Block Chaining) fixes this: before encrypting each block, it's XOR-ed with " +
            "the PREVIOUS block's ciphertext first (and the very first block is XOR-ed with a " +
            "random \"initialization vector,\" the IV). Now every block's output depends on every " +
            "block that came before it — two identical plaintext blocks encrypt to different " +
            "ciphertext, because their chaining history differs.",
            "The demo below runs real AES on text with a deliberately repeated chunk, under both " +
            "modes, and shows you each resulting 16-byte block. Watch what happens to the repeated " +
            "blocks."
        },
        BlockPattern = new BlockPatternDemo
        {
            InputLabel = "Message (try keeping the repeated phrase to see the pattern)",
            DefaultInput = "SECRETSECRETSECRETSECRETSECRET!",
            ComputeEcbBlocks = text => ToBlockHexList(ModernCrypto.AesEcbEncrypt(
                System.Text.Encoding.UTF8.GetBytes(text), ModernCrypto.DemoKeyFromPassphrase("tutorial-demo-key"))),
            ComputeCbcBlocks = text => ToBlockHexList(ModernCrypto.AesCbcEncrypt(
                System.Text.Encoding.UTF8.GetBytes(text), ModernCrypto.DemoKeyFromPassphrase("tutorial-demo-key"), ModernCrypto.ZeroIv16)),
            Explain = text =>
            {
                var ecb = ToBlockHexList(ModernCrypto.AesEcbEncrypt(System.Text.Encoding.UTF8.GetBytes(text), ModernCrypto.DemoKeyFromPassphrase("tutorial-demo-key")));
                var hasEcbRepeat = ecb.Count != ecb.Distinct().Count();
                return hasEcbRepeat
                    ? "Look at the ECB blocks: at least two of them are byte-for-byte identical, " +
                      "because the plaintext repeated. Now check the CBC blocks for the same " +
                      "repeated input — every single one is different, because each block's " +
                      "encryption was chained to everything before it."
                    : "This input doesn't repeat a full 16-byte block — try a longer repeated phrase " +
                      "(16+ identical characters) to see ECB's blocks visibly repeat.";
            }
        }
    };

    private static IReadOnlyList<string> ToBlockHexList(byte[] data)
    {
        var blocks = new List<string>();
        for (var i = 0; i < data.Length; i += 16)
        {
            var len = Math.Min(16, data.Length - i);
            blocks.Add(Convert.ToHexString(data, i, len).ToLowerInvariant());
        }
        return blocks;
    }

    // ---------------------------------------------------------------- 12. Authenticated encryption

    private static TutorialStep Step12AuthenticatedEncryption() => new()
    {
        Title = "CBC still isn't the whole story: authenticated encryption",
        Paragraphs = new[]
        {
            "CBC solves the pattern-leak problem, but it has its own sharp edges. It needs a " +
            "random, unpredictable IV for every single message (reusing one undoes the chaining " +
            "benefit for the first block), and — critically — CBC alone provides zero protection " +
            "against TAMPERING. An attacker who can flip bits in a CBC ciphertext can make " +
            "predictable, controlled changes to the decrypted plaintext, without needing the key at " +
            "all. Confidentiality (can't read it) and integrity (can't silently alter it) are " +
            "different properties, and CBC alone only gives you the first.",
            "Modern practice has mostly moved to modes built to give you both at once: GCM " +
            "(Galois/Counter Mode) is the big one — it's a stream-cipher-style mode (built on a " +
            "counter, conceptually similar to generating a one-time-pad-like keystream from the key " +
            "and a nonce) that ALSO produces an authentication tag, so any tampering with the " +
            "ciphertext is detected and rejected automatically. If you see \"AES-GCM\" in modern " +
            "documentation, this is why it's recommended over plain CBC.",
            "GCM has its own absolute rule, though, echoing Lesson 9 almost exactly: never reuse a " +
            "nonce with the same key. Do that even once and GCM's authentication guarantee " +
            "collapses completely — the exact same two-time-pad math from before, just wearing a " +
            "2020s outfit. You'll see this exact mistake as a Lab scenario."
        }
    };

    // ---------------------------------------------------------------- 13. Hashing vs encryption

    private static TutorialStep Step13HashingVsEncryption() => new()
    {
        Title = "A completely different tool: hashing",
        Paragraphs = new[]
        {
            "Everything so far has been encryption: two-way, reversible with the right key. " +
            "Hashing is different in kind, not just degree — a hash function takes input of any " +
            "size and produces a fixed-size fingerprint, with NO key, and NO way to reverse it back " +
            "to the original input. The same input always produces the same hash (deterministic), " +
            "but there's no \"decrypt\" operation at all — that's the point.",
            "This single confusion (\"hashing\" vs. \"encrypting\") is one of the most common " +
            "mix-ups in real engineering conversations. You encrypt data you need to read again " +
            "later. You hash data you only ever need to COMPARE — the classic case being a " +
            "password: the system never needs to know what your password \"really is,\" it only " +
              "ever needs to check whether what you typed just now matches what you typed when you " +
              "signed up."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Text to hash",
            DefaultInput = "hunter2",
            Transform = (input, _) => $"SHA-256: {ModernCrypto.Sha256Hex(input)}\nMD5:     {ModernCrypto.Md5Hex(input)}",
            Explain = (input, _) =>
                "Type the exact same text again and you'll get the exact same hash — every time, " +
                "forever, no key involved. There's genuinely no operation that turns this hash back " +
                "into \"hunter2\" — the only way to connect a hash back to a plaintext is to hash a " +
                "GUESS and see if it matches, which is exactly the next two lessons."
        }
    };

    // ---------------------------------------------------------------- 14. Salting

    private static TutorialStep Step14Salting() => new()
    {
        Title = "Hashing passwords safely, part 1: salting",
        Paragraphs = new[]
        {
            "Since a hash is deterministic, the same password always produces the same hash — " +
            "which means an attacker can pre-compute a giant table of common passwords and their " +
            "hashes ONE time (a \"rainbow table\") and then crack any stolen database instantly, " +
            "just by looking each hash up. Worse: if two users share a password, their hashes will " +
            "be identical, immediately leaking that fact to anyone who steals the database.",
            "A salt is a random value generated PER USER, PER PASSWORD, stored right next to the " +
            "hash (it's not secret), and mixed in before hashing: hash(salt + password). Two users " +
            "with the same password now get completely different hashes, because their salts " +
            "differ — and a pre-computed rainbow table (built for un-salted hashes) becomes " +
            "worthless, since it would need a separate table per salt, defeating the entire " +
            "economic point of pre-computing anything.",
            "Every time you click \"recompute\" below (or just re-type the same password), fresh " +
            "random salts get generated and the hashes come out different — proving the salt, not " +
            "the password, is what's changing."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Password (two \"different users\" will use this same one)",
            DefaultInput = "hunter2",
            Transform = (input, _) =>
            {
                var saltA = Convert.ToHexString(ModernCrypto.RandomSalt(4)).ToLowerInvariant();
                var saltB = Convert.ToHexString(ModernCrypto.RandomSalt(4)).ToLowerInvariant();
                return $"No salt (both users):  {ModernCrypto.Sha256Hex(input)}   <- identical for every user with this password\n" +
                       $"User A — salt {saltA}: {ModernCrypto.Sha256Hex(saltA + input)}\n" +
                       $"User B — salt {saltB}: {ModernCrypto.Sha256Hex(saltB + input)}";
            },
            Explain = (input, _) =>
                "The unsalted hash is identical for every user who picks this password — exactly " +
                "what makes rainbow tables and \"same password\" leaks possible. The two salted " +
                "hashes differ completely despite the IDENTICAL password, because each user's salt " +
                "is different. Re-run this and User A/B's salts (and hashes) change again — only " +
                "the unsalted line ever stays the same."
        }
    };

    // ---------------------------------------------------------------- 15. Slow KDFs

    private static TutorialStep Step15SlowKdf() => new()
    {
        Title = "Hashing passwords safely, part 2: slow, on purpose",
        Paragraphs = new[]
        {
            "Salting defeats PRE-COMPUTED attacks, but an attacker who steals your database can " +
            "still brute-force each salted hash individually, one guess at a time, offline, with no " +
            "rate limit. The problem: SHA-256 is FAST — it's literally designed for speed, since " +
            "it's also used for things like verifying large file downloads. A modern GPU can " +
            "compute billions of SHA-256 hashes per second.",
            "A proper password hash is a Key Derivation Function (KDF) — bcrypt, scrypt, or " +
            "PBKDF2 — deliberately engineered to be SLOW and, for scrypt/Argon2, also memory-" +
            "hungry, so the exact same hardware that computes billions of SHA-256 hashes a second " +
              "can only manage a few hundred or thousand KDF outputs a second instead. That " +
              "difference compounds across billions of brute-force guesses.",
            "The timing below is real, measured on your own machine right now — not a made-up " +
            "number. Watch the actual gap."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Password",
            DefaultInput = "hunter2",
            Transform = (input, _) =>
            {
                var swFast = Stopwatch.StartNew();
                const int fastIterations = 20_000;
                for (var i = 0; i < fastIterations; i++) ModernCrypto.Sha256Hex(input + i);
                swFast.Stop();

                var salt = ModernCrypto.RandomSalt();
                var swSlow = Stopwatch.StartNew();
                const int kdfIterations = 300_000;
                ModernCrypto.Pbkdf2Hex(input, salt, kdfIterations);
                swSlow.Stop();

                var perGuessFastMs = swFast.Elapsed.TotalMilliseconds / fastIterations;
                return $"{fastIterations:N0} raw SHA-256 guesses: {swFast.ElapsedMilliseconds} ms total (~{perGuessFastMs:F5} ms/guess)\n" +
                       $"ONE PBKDF2 guess ({kdfIterations:N0} iterations): {swSlow.ElapsedMilliseconds} ms\n" +
                       $"PBKDF2 is roughly {(swSlow.Elapsed.TotalMilliseconds / Math.Max(perGuessFastMs, 0.00001)):N0}x slower, PER GUESS, on this exact machine, right now.";
            },
            Explain = (input, _) =>
                "OWASP's 2023 minimum recommendation for PBKDF2-SHA256 is 600,000 iterations — this " +
                "demo uses fewer so it doesn't freeze the app while you type, but the real-world " +
                "ratio only gets more lopsided with the recommended setting. That multiplier applies " +
                "to every single guess in a brute-force attack — a crack that takes a day against " +
                "fast hashes can take years against a proper KDF."
        }
    };

    // ---------------------------------------------------------------- 16. HMAC

    private static TutorialStep Step16Hmac() => new()
    {
        Title = "Proving a message wasn't tampered with: HMAC",
        Paragraphs = new[]
        {
            "A plain hash proves nothing about WHO produced it — anyone can hash anything. An HMAC " +
            "(Hash-based Message Authentication Code) mixes in a shared secret key, so only " +
            "someone who knows that key could have produced that exact tag for that exact message. " +
            "Change even one character of the message, or use the wrong key, and the HMAC comes out " +
            "completely different.",
            "This is how you get real integrity protection: send the message alongside its HMAC, " +
            "and the receiver (who also knows the key) recomputes the HMAC themselves and checks it " +
            "matches. If an attacker altered the message in transit without knowing the key, their " +
            "tampering is caught instantly — they'd have no way to produce a matching HMAC for " +
            "their modified version.",
            "This is also precisely the ingredient CBC encryption is missing on its own (Lesson " +
            "12) — \"encrypt, then HMAC the ciphertext\" is a standard, safe pattern for bolting " +
            "integrity onto a cipher mode that doesn't provide it natively."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message",
            KeyLabel = "Shared secret key",
            DefaultInput = "transfer $100 to bob",
            DefaultKey = "shared-secret-key",
            Transform = (input, key) => string.IsNullOrEmpty(key) ? "Enter a key." : ModernCrypto.HmacSha256Hex(input, key),
            Explain = (input, key) =>
                "Try changing even one character of the message, or the key, and watch the entire " +
                "HMAC output change unpredictably — there's no \"partial match\" in cryptography; a " +
                "single altered bit anywhere produces a completely different, uncorrelated result."
        }
    };

    // ---------------------------------------------------------------- 17. Padding oracle

    private static TutorialStep Step17PaddingOracle() => new()
    {
        Title = "When two half-fixes combine into a real exploit: the padding oracle",
        Paragraphs = new[]
        {
            "This one ties together three earlier lessons at once. CBC-mode plaintext is padded " +
            "out to a whole number of 16-byte blocks before encryption (Lesson 11) — and when it's " +
            "decrypted, the receiver checks whether that padding looks valid before accepting the " +
            "message. If a server tells an attacker (even indirectly — through a different error " +
            "message, or a different response time) whether the padding on a tampered ciphertext " +
            "was valid or not, that's a \"padding oracle\": a yes/no answer about something the " +
            "attacker was never supposed to learn anything about.",
            "Here's the devastating part: with nothing but repeated access to that yes/no oracle " +
            "(no key, ever), an attacker can decrypt the ENTIRE original message, one byte at a " +
            "time, by resubmitting carefully corrupted versions of the ciphertext and watching " +
            "which ones \"pass\" the padding check. This is a real, repeatedly-exploited class of " +
            "vulnerability (it's what the 2011 \"POODLE\" and earlier ASP.NET padding-oracle " +
            "disclosures were built on) — entirely the result of CBC's missing integrity check " +
            "(Lesson 12) combined with an application that leaks whether decryption \"looked okay.\"",
            "The fix is the same HMAC pattern from the last lesson: authenticate the ciphertext " +
            "BEFORE attempting to decrypt and check its padding at all. If the HMAC doesn't match, " +
            "reject immediately and never even look at the padding — there's no oracle left to " +
            "query if the tampered message never reaches the padding check in the first place. " +
            "Modern authenticated modes like GCM (Lesson 12) close this off by design."
        }
    };

    // ---------------------------------------------------------------- 18. Key exchange

    private static TutorialStep Step18KeyExchange() => new()
    {
        Title = "The problem symmetric crypto can't solve by itself: key exchange",
        Paragraphs = new[]
        {
            "Every lesson so far assumed both sides already secretly share a key. In real life, " +
            "how do two people who've never met agree on a secret, over a connection an " +
            "eavesdropper can watch every bit of? That's the key-exchange problem, and it sounds " +
            "almost like a logical impossibility.",
            "Diffie-Hellman key exchange (1976) solved it with a clever trick from number theory: " +
            "modular exponentiation is easy to compute in one direction but extremely hard to " +
            "reverse (the \"discrete logarithm problem\"). Both sides publicly agree on a prime p " +
            "and a generator g, each picks a SECRET number, publishes gSECRET mod p, and then raises " +
            "the OTHER side's public value to their own secret power. Remarkably, both sides land on " +
            "the exact same number — without ever transmitting their private number, or the shared " +
            "secret itself, over the wire.",
            "The toy numbers below are far too small to be secure (real systems use numbers " +
            "hundreds of digits long specifically because the problem needs to stay hard even " +
            "against enormous computing power) — but the arithmetic is the real algorithm, just at " +
            "a demonstrable scale."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Alice's secret number",
            KeyLabel = "Bob's secret number",
            DefaultInput = "6",
            DefaultKey = "15",
            Transform = (aSecretStr, bSecretStr) =>
            {
                if (!int.TryParse(aSecretStr, out var aSecret) || !int.TryParse(bSecretStr, out var bSecret))
                    return "Enter whole numbers for both secrets.";

                const int p = 23, g = 5; // small public prime + generator, textbook toy example
                var aPublic = ModPow(g, aSecret, p);
                var bPublic = ModPow(g, bSecret, p);
                var aShared = ModPow(bPublic, aSecret, p);
                var bShared = ModPow(aPublic, bSecret, p);

                return $"Public: p={p}, g={g}  (these can be shouted across the room, no secrecy needed)\n" +
                       $"Alice publishes:  g^{aSecret} mod {p} = {aPublic}\n" +
                       $"Bob publishes:    g^{bSecret} mod {p} = {bPublic}\n" +
                       $"Alice computes:   {bPublic}^{aSecret} mod {p} = {aShared}\n" +
                       $"Bob computes:     {aPublic}^{bSecret} mod {p} = {bShared}\n" +
                       $"Shared secret matches: {(aShared == bShared ? "YES" : "no — check your inputs")}";
            },
            Explain = (_, _) =>
                "An eavesdropper watching p, g, and both PUBLIC values still can't easily recover " +
                "either secret number or the shared result — at real-world sizes (hundreds of " +
                "digits), reversing the modular exponentiation is believed to be computationally " +
                "infeasible. This exact idea, scaled up, is what lets your browser and a web server " +
                "agree on a session key before they've ever communicated before."
        }
    };

    private static long ModPow(long baseValue, long exponent, long modulus)
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

    // ---------------------------------------------------------------- 19. Randomness

    private static TutorialStep Step19Randomness() => new()
    {
        Title = "The ingredient everything above secretly depends on: randomness",
        Paragraphs = new[]
        {
            "Salts (Lesson 14), IVs (Lesson 11), nonces (Lesson 12), and every secret key in this " +
            "tutorial all depend on one unglamorous assumption: that \"random\" actually means " +
            "unpredictable. Most programming languages ship TWO different random number generators, " +
            "and mixing them up is a real, repeated source of vulnerabilities.",
            "A general-purpose PRNG (like .NET's plain `Random`, or JavaScript's `Math.random()`) " +
            "is built for speed and statistical distribution — great for shuffling a deck of cards " +
            "in a game, terrible for security. Critically, it can be SEEDED: the same seed always " +
            "produces the exact same sequence of \"random\" numbers, forever, which means if an " +
            "attacker can guess or narrow down the seed (often just \"the server's clock time when " +
            "it started\"), every value it ever produces becomes predictable.",
            "A cryptographically secure PRNG (.NET's `RandomNumberGenerator`) draws from the " +
            "operating system's actual entropy sources and makes no such promise of repeatability — " +
            "by design, you can't seed it, and you can't predict its next output even if you've " +
            "seen every previous one."
        },
        Demo = new CipherDemo
        {
            InputLabel = "A \"seed\" number to try",
            DefaultInput = "12345",
            Transform = (seedStr, _) =>
            {
                var seed = int.TryParse(seedStr, out var s) ? s : seedStr.GetHashCode();
                var r1 = new Random(seed).Next(100_000, 999_999);
                var r2 = new Random(seed).Next(100_000, 999_999);
                var csprng = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100_000, 999_999);
                return $"Seeded Random, run 1:        {r1}\n" +
                       $"Seeded Random, run 2 (same seed): {r2}   <- always identical, 100% predictable\n" +
                       $"RandomNumberGenerator (CSPRNG):   {csprng}   <- different every time, regardless of \"seed\"";
            },
            Explain = (_, _) =>
                "Two separately-created `Random` objects with the same seed produce the IDENTICAL " +
                "\"random\" number, every single time — that's not a bug, it's the documented, " +
                "intentional behavior, useful for reproducible simulations and utterly wrong for a " +
                "session token, password-reset code, or encryption key. The CSPRNG value changes " +
                "every time you click recompute, because it was never a function of a predictable " +
                "seed at all."
        }
    };

    // ---------------------------------------------------------------- 20. Wrap-up

    private static TutorialStep Step20WrapUp() => new()
    {
        Title = "You're ready — go break things on purpose",
        Paragraphs = new[]
        {
            "That's the whole conceptual arc: substitution leaks frequency, repetition leaks key " +
            "length, key reuse cancels out entirely, fixed block encryption leaks patterns, missing " +
            "integrity checks become exploitable oracles, fast hashing makes password cracking " +
            "cheap, and predictable randomness undermines every guarantee built on top of it.",
            "The Lab catalog takes every one of these lessons and shows them as realistic, " +
            "named real-world mistakes — hardcoded keys, reused nonces, ECB in production, " +
            "unsalted fast-hashed passwords, predictable tokens — each with a genuinely weak " +
            "\"Partial\" fix alongside the real \"Best\" fix, exactly like Minion.InjectionLab's " +
            "attack catalog. Quiz Mode tests you across all of it once you're comfortable.",
            "Have fun breaking things — on purpose, on your own machine, with your own toy data."
        }
    };
}
