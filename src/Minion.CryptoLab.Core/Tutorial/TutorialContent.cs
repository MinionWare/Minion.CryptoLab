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
        Step12bChaCha20Poly1305(),
        Step13HashingVsEncryption(),
        Step13bBrokenHashes(),
        Step14Salting(),
        Step14bPepper(),
        Step15SlowKdf(),
        Step16Hmac(),
        Step17PaddingOracle(),
        Step18KeyExchange(),
        Step18bPublicPrivateKeys(),
        Step18cDigitalSignatures(),
        Step18dCertificatesAndPki(),
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
            "the whole mechanism, and (spoiler) also exactly why it's breakable.",
            "One convention worth settling right now, since every cipher in this tutorial depends on " +
            "it: letters are numbered starting from ZERO — A=0, B=1, C=2, ... Z=25 — not A=1...Z=26. " +
            "This isn't arbitrary. Wrapping around the alphabet is a \"modulo 26\" operation (26 " +
            "letters, so one past Z wraps back to A), and that arithmetic is clean ONLY if your " +
            "values run 0 through 25. Shift 'Z' (25) by 3: (25+3) mod 26 = 28 mod 26 = 2, which is " +
            "'C' — correct, one line of math, no special cases.",
            "Now try the same wraparound with A=1...Z=26 instead. Shift 'Y' (25 in this scheme) by " +
            "1 — it should obviously give 'Z'. The same formula gives (25+1) mod 26 = 26 mod 26 = 0. " +
            "But there IS no letter 0 in a 1-26 scheme — the alphabet starts at 1. The math silently " +
            "produces a value that doesn't correspond to anything, right at the exact moment the " +
            "wraparound happens, and you'd need to bolt on a special case (\"if the result is 0, it " +
            "actually means 26/Z\") to every single shift calculation, forever, just to patch over " +
            "it. Zero-indexing doesn't have that gap at all — every possible result of a mod-26 " +
            "operation (0 through 25) is automatically a real letter, with nothing left over to " +
            "patch. It's a small choice that quietly removes an entire category of off-by-one bugs."
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
            "## ROT13",
            "ROT13 is just a Caesar shift of exactly 13 — nothing new mathematically, just the same " +
            "(position + shift) mod 26 formula from Lesson 2 with the shift locked to 13 forever. " +
            "Spelled out letter by letter on the word \"CIPHER\" (position numbers from the same " +
            "A=0...Z=25 scheme as always):",
            "Letter:        C    I    P    H    E    R\n" +
            "Position:      2    8    15   7    4    17\n" +
            "+13:           15   21   28   20   17   30\n" +
            "mod 26:        15   21   2    20   17   4\n" +
            "Result:        P    V    C    U    R    E",
            "So ROT13(\"CIPHER\") = \"PVCURE\". Notice position 15 (\"P\" in \"+13\" row, from " +
            "letter 'C') wrapped around at 28 mod 26 = 2, landing back on 'C' — same wraparound " +
            "math as Caesar, just always by exactly 13.",
            "Here's why applying it TWICE undoes it, and it's simple arithmetic, not magic: 13+13=26, " +
            "and 26 mod 26 = 0 — shifting by 26 is the same as shifting by 0, i.e. not shifting at " +
            "all. Run ROT13 on \"PVCURE\" (the output above) and check: 'P' is position 15, 15+13=28, " +
            "28 mod 26 = 2 = 'C' — back to the start. Do that for all six letters and you land " +
            "exactly back on \"CIPHER\", because every single letter made the same round trip: +13, " +
            "then +13 again, totaling +26, totaling +0. It's still used today on forums to hide " +
            "spoilers or puzzle answers from casual glances — it was never meant to be secure, and " +
            "the moment someone knows it's ROT13, there's no key left to find.",
            "## Atbash",
            "Atbash is older (it appears in the Hebrew Bible) and uses a different formula: instead " +
            "of adding a shift, it reflects each position across the middle of the alphabet — " +
            "position p maps to position (25 - p), so A(0)<->Z(25), B(1)<->Y(24), and so on inward. " +
            "Same word, same letter-by-letter breakdown:",
            "Letter:        C    I    P    H    E    R\n" +
            "Position (p):  2    8    15   7    4    17\n" +
            "25 - p:        23   17   10   18   21   8\n" +
            "Result:        X    R    K    S    V    I",
            "Atbash(\"CIPHER\") = \"XRKSVI\". It undoes itself for an even simpler reason than ROT13: " +
            "25 - (25 - p) = p, always, for any p — reflecting a reflection lands you back where you " +
            "started, by definition, with no modular wraparound even needed. Check it on 'X' " +
            "(position 23): 25-23=2='C', the original letter. Neither ROT13 nor Atbash has a \"key\" " +
            "you can change at all — the rule itself is the entire secret, which is an even weaker " +
            "design than Caesar's adjustable shift."
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
            "Why stop at shifting the alphabet by one fixed amount? A simple substitution cipher " +
            "scrambles the WHOLE alphabet into any order at all — not \"shift everything by N,\" but " +
            "\"a comes out as whatever letter I want, b comes out as whatever letter I want, " +
            "independently.\" The key is the full scrambled alphabet itself, 26 letters long. " +
            "Caesar's shift is really just one very narrow, predictable kind of substitution (the " +
            "scramble is always \"shift everything the same amount\") — this is the fully general " +
            "version, where each letter's replacement is picked independently.",
            "## A worked example",
            "Key \"QWERTYUIOPASDFGHJKLZXCVBNM\" (26 letters, a scrambled alphabet — read it as " +
            "\"a maps to whatever's in position 0 of this key, b maps to position 1,\" and so on). " +
            "Encrypt \"CAT\", letter by letter, by looking up each letter's POSITION (same " +
            "A=0...Z=25 numbering as always) in that key string:",
            "Letter:              C         A         T\n" +
            "Position:            2         0         19\n" +
            "Key string index:    key[2]    key[0]    key[19]\n" +
            "Key letter there:    E         Q         Z",
            "So \"CAT\" becomes \"EQZ\" — 'C' (position 2) became whatever sits at INDEX 2 of the key " +
            "(\"E\"), 'A' (position 0, the very first slot) became index 0 of the key (\"Q\"), and " +
            "'T' (position 19) became index 19 of the key (\"Z\"). Every letter of the message looks " +
            "up its own position in that one 26-letter key string — that's the entire cipher.",
            "## Building the keyspace number",
            "Here's the number that should sound reassuring, and — unlike earlier lessons — let's " +
            "actually build it instead of just stating it. The key is a scrambled alphabet, so " +
            "picking one means deciding what 'A' maps to, then what 'B' maps to, and so on. 'A' can " +
            "map to any of the 26 letters — 26 choices. Once that letter is used up, 'B' can map to " +
            "any of the 25 REMAINING letters — 25 choices. 'C' then has 24 remaining choices, 'D' " +
            "has 23, and so on down to the very last letter, 'Z,' which has only 1 letter left " +
            "(whichever one nothing else took). Multiply every one of those choice-counts together — " +
            "26 x 25 x 24 x ... x 2 x 1 — and that product has an abbreviation: 26! (\"26 factorial\"). " +
            "It works out to roughly 400,000,000,000,000,000,000,000,000 possible keys. Trying every " +
            "single one, even at a billion attempts per second, would take far longer than the " +
            "universe has existed. Surely THIS is unbreakable?",
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
                "scrambled alphabet — exactly the \"CAT\" -> \"EQZ\" lookup worked out above, just " +
                "run across your whole message instead of 3 letters. With 26! possible keys, " +
                "brute-forcing every arrangement is completely impractical — which is exactly why " +
                "real attacks on this cipher never try to guess the key directly at all."
        }
    };

    // ---------------------------------------------------------------- 5. Frequency analysis

    private static TutorialStep Step05FrequencyAnalysis() => new()
    {
        Title = "The attack that doesn't care about keyspace: frequency analysis",
        Paragraphs = new[]
        {
            "In ordinary English text, the letter E shows up far more often than any other letter — " +
            "roughly 12-13 out of every 100 letters (about 1 in 8). The next most common, " +
            "approximately: T (9%), A (8%), O (7.5%), I (7%), N (6.7%) — typesetters memorized this " +
            "order as \"ETAOIN SHRDLU.\" A simple substitution cipher swaps letters one-for-one, so " +
            "it can hide WHICH letter means \"E\" — but it can't hide the fact that SOME letter in " +
            "the ciphertext is going to show up roughly 1-in-8 times, because that letter is " +
            "standing in for E on every single occurrence, every single time.",
            "## A worked example",
            "Take \"THE TREE NEEDS WATER\" — a short sentence, chosen because it happens to use the " +
            "letter E six times. Encrypt it with a Caesar shift of 3 (Lesson 2's cipher, picked only " +
            "because it's the simplest possible substitution — the argument below works identically " +
            "for the fully scrambled kind from Lesson 4):",
            "Plaintext:  THE TREE NEEDS WATER\n" +
            "Ciphertext: WKH WUHH QHHGV ZDWHU",
            "Now just COUNT the letters in that ciphertext, ignoring what they actually decrypt to — " +
            "pretend you don't know the key at all:",
            "H: 6   W: 3   U: 2   K: 1   Z: 1   Q: 1   G: 1   D: 1   V: 1",
            "'H' towers over every other letter, 6 occurrences against 3 or fewer for everything " +
            "else — and 'H' is exactly the letter E became under this key (E is position 4, " +
            "4+3=7='H', same formula as every Caesar example so far). You didn't need the key, or " +
            "even a guess at the shift amount, to notice 'H' is almost certainly standing in for E — " +
            "the SHAPE of the letter-count table gave it away before any decryption happened.",
            "Scale that same counting trick up to a real paragraph of ciphertext, instead of one " +
            "sentence, and the pattern gets even sharper and more reliable (short examples like the " +
            "one above can have a lucky or unlucky letter or two; a few hundred letters of real " +
            "English essentially never do). Count the letters in a long enough ciphertext, find the " +
            "most common one, guess it's probably E, find the second-most common, guess T — and the " +
            "26!-sized keyspace from Lesson 4 becomes irrelevant. You never tried a single key. You " +
            "read the shape of the language straight through the disguise.",
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
            "different Caesar shift for each letter in turn — letter 1 of the message shifts by " +
            "letter 1 of the key, letter 2 by letter 2 of the key, and so on. Once the key runs out " +
            "of letters, it just starts over from its own beginning and keeps going, for as long as " +
            "the message runs. Each key letter means \"shift by this many places,\" exactly like a " +
            "Caesar shift: A=0, B=1, C=2, ... Z=25.",
            "## Worked example 1: \"ATTACKATDAWN\" / \"LEMON\"",
            "First, the key itself, converted to numbers the same A=0...Z=25 way — this is the " +
            "ENTIRE source of every shift below, nothing about it is random:",
            "L=11   E=4   M=12   O=14   N=13",
            "Here's the first example spelled out completely, letter by letter, key \"LEMON\" lined " +
            "up under message \"ATTACKATDAWN\" (the key repeats: L-E-M-O-N-L-E-M-O-N-L-E once it " +
            "runs out at letter 6):",
            "Plaintext:  A   T   T   A   C   K   A   T   D   A   W   N\n" +
            "Key letter: L   E   M   O   N   L   E   M   O   N   L   E\n" +
            "Shift by:   11  4   12  14  13  11  4   12  14  13  11  4\n" +
            "Ciphertext: L   X   F   O   P   V   E   F   R   N   H   R",
            "Read any one column top to bottom: plaintext 'A' (position 0 in the alphabet) shifted " +
            "by key letter 'L' (11, from the legend above) lands on position 0+11=11, which is 'L'. " +
            "Next column: 'T' (position 19) shifted by 'E' (4) lands on 19+4=23, which is 'X'. Keep " +
            "going the same way for every column and you get the whole ciphertext: LXFOPVEFRNHR.",
            "Look at the \"Shift by\" row again: 11, 4, 12, 14, 13, 11, 4, 12, 14, 13, 11, 4 — that's " +
            "just L, E, M, O, N's numbers (11, 4, 12, 14, 13) repeating from the start, over and " +
            "over, because the 5-letter key ran out and cycled back to its own beginning. It repeats " +
            "every 5 columns because the key is 5 letters long — with only 12 columns shown here " +
            "that's 2 full cycles plus 2 extra, which can look irregular at a glance if you're not " +
            "counting in fives. There is nothing random anywhere in this table: every single shift " +
            "is 100% determined by which key letter happens to land in that column.",
            "Notice plaintext 'A' shows up twice (columns 1 and 4) and becomes TWO DIFFERENT " +
            "ciphertext letters ('L' and 'P') each time, because a different key letter was " +
            "lined up underneath it both times. That's the entire mechanism that defeats plain " +
            "frequency analysis — the same input letter no longer reliably becomes the same output " +
            "letter.",
            "## Worked example 2: \"HELLOWORLD\" / \"KEY\"",
            "One more full example, different words, so the pattern is unmistakable — message " +
            "\"HELLOWORLD\" with the short 3-letter key \"KEY\" (repeating as K-E-Y-K-E-Y-K-E-Y-K, " +
            "three full times plus one letter). Key legend first: K=10, E=4, Y=24 — and that's it, " +
            "only three distinct shift values exist anywhere in this entire example:",
            "Plaintext:  H   E   L   L   O   W   O   R   L   D\n" +
            "Key letter: K   E   Y   K   E   Y   K   E   Y   K\n" +
            "Shift by:   10  4   24  10  4   24  10  4   24  10\n" +
            "Ciphertext: R   I   J   V   S   U   Y   V   J   N",
            "'H' (position 7) shifted by 'K' (10) is 7+10=17 → 'R'. 'L' (position 11) appears three " +
            "times in this message and shifts by 'Y' (24), 'K' (10), and 'Y' (24) in turn — landing " +
            "on 'J', 'V', and 'J' again. Same plaintext letter, three different ciphertext letters, " +
            "for exactly the same reason as the first example: a short key marching in lockstep " +
            "with a longer message, wrapping back to its own start over and over. The \"Shift by\" " +
            "row here is just 10, 4, 24 repeating every 3 columns — again, a 3-letter key, a " +
            "3-column period, zero randomness.",
            "This single idea defeated simple frequency analysis for over 300 years and earned the " +
            "nickname \"le chiffre indéchiffrable\" — the indecipherable cipher."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message (letters only — spaces/punctuation won't line up cleanly in the table below)",
            KeyLabel = "Keyword",
            DefaultInput = "ATTACKATDAWN",
            DefaultKey = "LEMON",
            Transform = (input, key) => string.IsNullOrEmpty(key) ? "Enter a keyword." : ClassicalCiphers.Vigenere(input, key),
            Explain = (input, key) => string.IsNullOrEmpty(key) ? "Enter a keyword to see the letter-by-letter breakdown." : VigenereAlignmentTable(input, key)
        }
    };

    /// <summary>Renders the same Plaintext/Key/Shift/Ciphertext alignment table shown in the
    /// worked examples above, for whatever the reader actually types — letters only, so the
    /// columns stay lined up.</summary>
    private static string VigenereAlignmentTable(string plaintext, string key)
    {
        var letters = plaintext.Where(char.IsLetter).Select(char.ToUpperInvariant).ToList();
        if (letters.Count == 0) return "Type a message using letters to see the breakdown.";

        var keyUpper = key.ToUpperInvariant();
        var keyLetters = Enumerable.Range(0, letters.Count).Select(i => keyUpper[i % keyUpper.Length]).ToList();
        var shifts = keyLetters.Select(k => k - 'A').ToList();
        var cipherLetters = Enumerable.Range(0, letters.Count)
            .Select(i => (char)('A' + ((letters[i] - 'A' + shifts[i]) % 26)))
            .ToList();

        string Row(string label, IEnumerable<string> cells) => $"{label,-12}" + string.Join(" ", cells.Select(c => c.PadRight(3)));

        var distinctKeyLetters = keyUpper.Distinct().OrderBy(c => keyUpper.IndexOf(c));
        var legend = string.Join("   ", distinctKeyLetters.Select(c => $"{c}={c - 'A'}"));

        return $"Key legend: {legend}\n\n" +
               Row("Plaintext:", letters.Select(c => c.ToString())) + "\n" +
               Row("Key letter:", keyLetters.Select(c => c.ToString())) + "\n" +
               Row("Shift by:", shifts.Select(s => s.ToString())) + "\n" +
               Row("Ciphertext:", cipherLetters.Select(c => c.ToString())) + "\n\n" +
               $"The \"Shift by\" row is just those {keyUpper.Distinct().Count()} numbers from the " +
               $"legend above, repeating every {keyUpper.Length} column(s) — nothing random, it's " +
               "entirely determined by which key letter cycles into that position.";
    }

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
            "Everything from here on works on BITS, not letters — the 0s and 1s a computer actually " +
            "stores. If you've never worked with bits or binary before, start here, slowly, because " +
            "every remaining lesson in this tutorial builds on this one operation.",

            "## The XOR rule",
            "XOR (\"exclusive or\") takes two single bits and produces one bit back. It asks exactly " +
            "one question: ARE THE TWO INPUT BITS DIFFERENT? If yes, the result is 1. If they're the " +
            "same, the result is 0. That is the ENTIRE rule — there is nothing else to it. Written " +
            "out as every possible case (only four exist, since a bit is only ever 0 or 1):",
            "  Bit A   Bit B   A XOR B   (same as each other?)\n" +
            "    0       0        0      yes -> 0\n" +
            "    0       1        1      no  -> 1\n" +
            "    1       0        1      no  -> 1\n" +
            "    1       1        0      yes -> 0",

            "## Why XOR-ing twice undoes itself",
            "Now the part that makes XOR useful for a cipher: XOR-ing a result with the SAME bit " +
            "again always undoes it, for both possible keys, with no exceptions — because there are " +
            "only four cases total, we can just check every single one, by hand:",
            "  Plain(P)  Key(K)  Encrypt: C=P XOR K  Decrypt: C XOR K   Got P back?\n" +
            "     0        0            0                 0               yes\n" +
            "     0        1            1                 0               yes\n" +
            "     1        0            1                 1               yes\n" +
            "     1        1            0                 1               yes",
            "Look at the \"Decrypt\" column: it's identical to the original \"Plain\" column, every " +
            "single time, no matter what P or K were. That's not a coincidence that happens to work " +
            "for these numbers — it's EVERY possible case for a single bit, so there's nothing left " +
            "to be wrong. That is the entire mathematical proof that \"XOR the same key twice\" always " +
            "gets you back to where you started.",

            "## Scaling up to a whole byte",
            "A letter isn't one bit, though — it's a whole BYTE, 8 bits glued together, and XOR just " +
            "does the exact same single-bit operation 8 times, once per matching position, completely " +
            "independently. Take the letter 'H' and XOR it with the letter 'o' used as a one-byte " +
            "\"key\" — first, their bits (every letter has a fixed binary code; 'H' is always " +
            "01001000, 'o' is always 01101111):",
            "  'H' (plaintext):  0 1 0 0 1 0 0 0\n" +
            "  'o' (key):        0 1 1 0 1 1 1 1\n" +
            "  XOR, bit by bit:  0 0 1 0 0 1 1 1   <- compare each column above using the table higher up",
            "Read the 8 result bits left to right as one byte: 00100111. Converted from binary, that's " +
            "the number 39, which is the character ' (an apostrophe) — scrambled, unreadable as " +
            "'H', exactly what we want from encryption.",
            "Now undo it — decrypt by XOR-ing that SAME result with the SAME key again, bit by bit, " +
            "no different operation needed:",
            "  00100111 (ciphertext, the apostrophe):  0 0 1 0 0 1 1 1\n" +
            "  'o' (the same key again):                0 1 1 0 1 1 1 1\n" +
            "  XOR, bit by bit:                         0 1 0 0 1 0 0 0",
            "Read those 8 bits: 01001000 = 72 = 'H'. Back to the original letter, using the identical " +
            "XOR operation both times — nothing marked \"decrypt mode,\" nothing reversed, just XOR " +
            "with the same key a second time.",

            "## Scaling up to a short string",
            "One letter is a good start, but a real message is a short STRING of letters, each one " +
            "XOR-ed with its own key byte in the same position. Keeping it short on purpose: encrypt " +
            "\"HI\" (2 letters) with the 2-letter key \"ok\" — 'H' pairs with 'o' (worked out above, " +
            "giving the apostrophe), and 'I' pairs with 'k':",
            "  'I' (plaintext):  0 1 0 0 1 0 0 1\n" +
            "  'k' (key):        0 1 1 0 1 0 1 1\n" +
            "  XOR, bit by bit:  0 0 1 0 0 0 1 0",
            "That byte, 00100010, is 34 — the character \" (a double quote). So \"HI\" XOR \"ok\", " +
            "letter by letter, byte by byte, gives you two punctuation marks: the apostrophe from " +
            "before, then this double quote — ciphertext \"'\\\"\" (both characters together). Nothing " +
            "about those symbols looks like \"HI\" at all, yet every bit of the transformation was " +
            "just the same four-row table from the very top of this lesson, applied 16 times (8 bits " +
            "x 2 letters).",
            "Decrypting the string works exactly like decrypting the single letter: XOR each " +
            "ciphertext byte with the SAME key byte that encrypted it, in the SAME position. " +
            "apostrophe XOR 'o' gives back 'H' (already shown above); double-quote XOR 'k' gives " +
            "back 'I' the same way. Two letters, two independent 8-bit XOR operations, each one " +
            "undoing itself for exactly the reason the single-bit table proved.",
            "XOR-ing data with a repeating key, one byte at a time — reusing \"ok\" over and over for " +
            "a longer message the way \"LEMON\" repeated over and over in Vigenère — is the EXACT same " +
            "idea as Vigenère, just operating on raw bits instead of letters-mod-26. The live demo " +
            "below does exactly this, on whatever you type, and shows you the real ciphertext bytes " +
            "as hex.",

            "## The one-time pad",
            "Now take the repeating-key idea to its logical extreme: what if the \"key\" is exactly " +
            "as long as the message, made of truly random bits, and used exactly once, ever — never " +
            "repeated at all? That's the one-time pad — and it is mathematically, provably " +
            "unbreakable. Not \"unbreakable with today's computers\" — unbreakable, full stop, even " +
            "with infinite computing power, because every possible plaintext is equally consistent " +
            "with the ciphertext.",
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
            {
                if (string.IsNullOrEmpty(key) || input.Length == 0) return "Enter a message and a key.";
                var p = (byte)input[0];
                var k = (byte)key[0];
                var c = (byte)(p ^ k);
                return "This is raw bytes now, shown as hex, not letters — XOR doesn't respect the " +
                       "alphabet the way Caesar/Vigenère do, it just flips bits. Exactly like the " +
                       "'H'/'o' example above, here's your own first letter worked out the same way:\n\n" +
                       $"  '{(char)p}' (plaintext):  {ToBitsSpaced(p)}\n" +
                       $"  '{(char)k}' (key):        {ToBitsSpaced(k)}\n" +
                       $"  XOR, bit by bit:  {ToBitsSpaced(c)}   = 0x{c:x2} (first byte of the hex above)\n\n" +
                       "If this key were truly random, exactly as long as the message, and used only " +
                       "this once, this ciphertext would be a genuine one-time pad: provably unbreakable.";
            }
        }
    };

    private static string ToBitsSpaced(byte b) => string.Join(" ", Convert.ToString(b, 2).PadLeft(8, '0').ToCharArray());

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
            "completely, XOR-ed with itself, leaving the XOR of the two original messages. That " +
            "algebra isn't just symbol-shuffling — it's Lesson 8's single-bit proof (XOR-ing a value " +
            "with itself always gives 0, and XOR-ing anything with 0 leaves it unchanged) applied to " +
            "a whole byte instead of one bit. Here it is with real numbers, one letter each, same " +
            "hidden key byte 10011001 (0x99) reused for both — exactly the mistake, right here:",
            "  'A' (P1):        0 1 0 0 0 0 0 1\n" +
            "  Key (K):         1 0 0 1 1 0 0 1\n" +
            "  C1 = P1 XOR K:   1 1 0 1 1 0 0 0   (0xd8)\n" +
            "\n" +
            "  'B' (P2):        0 1 0 0 0 0 1 0\n" +
            "  Key (K):         1 0 0 1 1 0 0 1\n" +
            "  C2 = P2 XOR K:   1 1 0 1 1 0 1 1   (0xdb)\n" +
            "\n" +
            "  C1 XOR C2:       0 0 0 0 0 0 1 1   (0x03)\n" +
            "  P1 XOR P2:       0 0 0 0 0 0 1 1   (0x03)   <- identical to C1 XOR C2, with no key involved",
            "The attacker computing \"C1 XOR C2\" never touched K at all, yet landed on the exact " +
            "same byte as \"P1 XOR P2\" — because K appeared in both C1 and C2, and XOR-ing those two " +
            "together cancels it out the same way any value XOR'd with itself does. That's not the " +
            "plaintext yet, but it's no longer random-looking noise either — it's structured data an " +
            "attacker can prise apart using the same letter-frequency thinking from Lesson 5, a " +
            "technique called \"crib-dragging\" (guessing a likely word in one message and sliding it " +
            "along to see what it reveals in the other).",
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
        },
        Demo = new CipherDemo
        {
            InputLabel = "A sample message",
            DefaultInput = "Meet me at the usual place tonight",
            Transform = (input, _) =>
            {
                var len = System.Text.Encoding.UTF8.GetByteCount(input);
                var fullBlocks = len / 16;
                var remainder = len % 16;
                var blocksNeeded = (len + 15) / 16;
                return $"Message length: {len} bytes\n" +
                       $"{len} / 16 = {fullBlocks} full 16-byte block(s), remainder {remainder} byte(s)\n" +
                       (remainder == 0
                           ? $"Divides evenly — exactly {fullBlocks} block(s), nothing left over."
                           : $"That leftover {remainder}-byte chunk is NOT a full block, but AES only " +
                             $"knows how to scramble whole 16-byte blocks — so this message needs " +
                             $"{blocksNeeded} blocks total, with the last one short by {16 - remainder} " +
                             "byte(s) that have to come from somewhere (that \"somewhere\" is called " +
                             "padding — covered in the next lesson).");
            },
            Explain = (input, _) =>
                "This is exactly the question a block cipher forces on you for every single message: " +
                "how many whole 16-byte blocks does this take, and what happens to whatever doesn't " +
                "fit evenly? A stream cipher (like the repeating-key XOR from Lesson 8, or AES used " +
                "in a stream-like mode) never asks this question at all — it just generates exactly " +
                "as much keystream as the message needs, one byte at a time, so there's never a " +
                "leftover chunk to deal with. That tradeoff — a block cipher's extra bookkeeping in " +
                "exchange for different security properties — is exactly what the next lesson digs " +
                "into."
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
            "ciphertext, because their chaining history differs. \"Depends on\" shouldn't be taken " +
            "on faith — here's the exact chain, computed for real, with real AES, so you can check " +
            "every step yourself:",
            CbcWalkthrough(),
            "Notice block 1's input (\"IV XOR P1\") came out identical to P1 itself — that's because " +
            "this walkthrough deliberately used an all-zero IV, so you could follow the arithmetic " +
            "without a third random-looking value to track. But that's also a real, serious mistake: " +
            "an all-zero (or any fixed, reused) IV means the FIRST block of CBC gets no chaining " +
            "protection at all — two different messages that happen to start with the same 16 bytes " +
            "would produce the exact same first ciphertext block, exactly the pattern-leak CBC exists " +
            "to prevent. A real IV must be random and different for every single message — the live " +
            "demo below reuses a zero IV too, purely so its output stays reproducible for this " +
            "lesson, never do this in real code.",
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

    /// <summary>A real, computed, block-by-block CBC trace: IV XOR P1 -> AES -> C1, then
    /// C1 XOR P2 -> AES -> C2. Uses a raw single-block AES encryptor (no padding) since each step
    /// here really is exactly one 16-byte block — confirmed against .NET's own CBC implementation
    /// to produce byte-identical output before this was written up.</summary>
    private static string CbcWalkthrough()
    {
        var key = ModernCrypto.DemoKeyFromPassphrase("tutorial-demo-key");
        var iv = ModernCrypto.ZeroIv16;
        var p1 = System.Text.Encoding.UTF8.GetBytes("ATTACKATDAWNNOW!");
        var p2 = System.Text.Encoding.UTF8.GetBytes("SENDMORETROOPS!!");

        var block1Input = ClassicalCiphers.Xor(iv, p1);
        var c1 = AesRawBlockEncrypt(block1Input, key);
        var block2Input = ClassicalCiphers.Xor(c1, p2);
        var c2 = AesRawBlockEncrypt(block2Input, key);

        string H(byte[] b) => Convert.ToHexString(b).ToLowerInvariant();
        string Row(string label, string value) => label.PadRight(38) + value;

        return string.Join("\n", new[]
        {
            Row("IV (all zero — see the note below):", H(iv)),
            Row("Plaintext block 1 (\"ATTACKATDAWNNOW!\"):", H(p1)),
            Row("Block 1 input = IV XOR P1:", H(block1Input)),
            Row("Ciphertext block 1 = AES(input):", H(c1)),
            "",
            Row("Plaintext block 2 (\"SENDMORETROOPS!!\"):", H(p2)),
            Row("Block 2 input = C1 XOR P2:", H(block2Input)),
            Row("Ciphertext block 2 = AES(input):", H(c2))
        });
    }

    /// <summary>Encrypts exactly one 16-byte block with no padding — used only to show CBC's
    /// internal chaining math one block at a time (real AesCbcEncrypt is used everywhere else).</summary>
    private static byte[] AesRawBlockEncrypt(byte[] block, byte[] key)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = key;
        aes.Mode = System.Security.Cryptography.CipherMode.ECB;
        aes.Padding = System.Security.Cryptography.PaddingMode.None;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(block, 0, block.Length);
    }

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

    private static readonly byte[] GcmDemoKey = ModernCrypto.DemoKeyFromPassphrase("tutorial-demo-key");
    private static readonly byte[] GcmDemoNonce = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }; // 12 bytes — GCM's standard nonce size

    private static TutorialStep Step12AuthenticatedEncryption() => new()
    {
        Title = "CBC still isn't the whole story: authenticated encryption",
        Paragraphs = new[]
        {
            "CBC solves the pattern-leak problem, but it has its own sharp edges. It needs a " +
            "random, unpredictable IV for every single message (Lesson 11's closing note showed " +
            "exactly why), and — critically — CBC alone provides zero protection against TAMPERING. " +
            "An attacker who can flip bits in a CBC ciphertext can make predictable, controlled " +
            "changes to the decrypted plaintext, without needing the key at all. Confidentiality " +
            "(can't read it) and integrity (can't silently alter it) are different properties, and " +
            "CBC alone only gives you the first.",
            "Modern practice has mostly moved to modes built to give you both at once: GCM " +
            "(Galois/Counter Mode) is the big one — internally it's a stream-cipher-style mode: a " +
            "keystream is generated from the key plus a per-message NONCE (\"number used once\" — " +
            "the GCM term for what CBC calls an IV) and XOR-ed with the plaintext, conceptually a " +
            "one-time pad where the \"random\" key comes from AES instead of true randomness. It ALSO " +
            "computes an authentication tag over the ciphertext, so any tampering is detected and " +
            "rejected automatically — the exact protection CBC alone doesn't have.",
            "And because it's really just a keystream XOR, GCM inherits the two-time-pad math from " +
            "Lesson 9 exactly, bug for bug: reuse the same key+nonce pair for two different " +
            "messages and the keystream cancels out of their ciphertexts the same way a reused " +
            "one-time-pad key did. No tag, no CBC, no AES internals involved in seeing it happen — " +
            "just real AES-GCM, run twice with the mistake baked in:"
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message 1",
            KeyLabel = "Message 2 (both \"encrypted\" with the SAME hidden AES-GCM key AND the same reused nonce)",
            DefaultInput = "TRANSFER 100 TO BOB ACCOUNT NOW",
            DefaultKey = "MEETING MOVED TO THREE PM TODAY",
            Transform = (p1, p2) =>
            {
                var b1 = System.Text.Encoding.UTF8.GetBytes(p1);
                var b2 = System.Text.Encoding.UTF8.GetBytes(p2);
                var c1 = ModernCrypto.AesGcmEncrypt(b1, GcmDemoKey, GcmDemoNonce, out _);
                var c2 = ModernCrypto.AesGcmEncrypt(b2, GcmDemoKey, GcmDemoNonce, out _);
                var len = Math.Min(c1.Length, c2.Length);
                var xorOfCiphertexts = ClassicalCiphers.Xor(c1[..len], c2[..len]);
                var xorOfPlaintexts = ClassicalCiphers.Xor(b1[..len], b2[..len]);
                var cHex = Convert.ToHexString(xorOfCiphertexts).ToLowerInvariant();
                var pHex = Convert.ToHexString(xorOfPlaintexts).ToLowerInvariant();
                return $"C1 (hex):              {Convert.ToHexString(c1).ToLowerInvariant()}\n" +
                       $"C2 (hex):              {Convert.ToHexString(c2).ToLowerInvariant()}\n" +
                       $"C1 XOR C2:             {cHex}\n" +
                       $"P1 XOR P2 (for proof): {pHex}\n" +
                       $"Identical? {(cHex == pHex ? "YES — same catastrophic key-reuse failure as Lesson 9, just with real AES-GCM" : "no")}";
            },
            Explain = (_, _) =>
                "This only shows the ENCRYPTION half breaking — reusing a GCM nonce also destroys " +
                "the authentication tag's security (it reuses the same internal \"hash key\" GCM " +
                "derives for its tag, letting an attacker forge valid tags for altered ciphertext), " +
                "but that math needs Galois-field arithmetic this tutorial won't derive. The " +
                "keystream cancellation above is the real, computable half of why \"never reuse a " +
                "nonce\" is as absolute a rule for GCM as \"never reuse a one-time pad key\" was back " +
                "in Lesson 9 — because underneath, it's the identical mistake."
        }
    };

    // ---------------------------------------------------------------- 12b. ChaCha20-Poly1305

    private static readonly byte[] ChaChaDemoKey = ModernCrypto.DemoKeyFromPassphrase("tutorial-demo-key-chacha");
    private static readonly byte[] ChaChaDemoNonce = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

    private static TutorialStep Step12bChaCha20Poly1305() => new()
    {
        Title = "The other modern cipher: ChaCha20-Poly1305",
        Paragraphs = new[]
        {
            "AES-GCM (last lesson) is extremely fast on hardware with built-in AES acceleration — " +
            "which is most modern PCs and servers. But not every device has that: older phones, " +
            "cheap IoT chips, and some embedded hardware don't, and AES runs noticeably slower there " +
            "in plain software. ChaCha20 is a different stream cipher, built entirely out of basic " +
            "operations (add, rotate, XOR) that run fast on ANY processor with no special hardware " +
            "needed — which is exactly why it's the default cipher for a lot of mobile HTTPS " +
            "traffic today.",
            "Poly1305 is the authentication half, paired with ChaCha20 the same way GCM pairs AES " +
            "with its own authentication tag. Together, \"ChaCha20-Poly1305\" is a complete AEAD " +
            "(authenticated encryption) — the same CATEGORY of tool as AES-GCM, solving the exact " +
            "same two problems (confidentiality + tamper detection) at once, just built from " +
            "different internal machinery.",
            "And because it's fundamentally the same idea as GCM underneath — a keystream generated " +
            "from the key and nonce, XOR-ed with the plaintext — it inherits the EXACT same rule, " +
            "for the exact same reason: never reuse a nonce with the same key. Here's that rule " +
            "breaking live, with real ChaCha20-Poly1305 instead of AES-GCM, same two messages, same " +
            "reused-nonce mistake:"
        },
        Demo = new CipherDemo
        {
            InputLabel = "Message 1",
            KeyLabel = "Message 2 (both \"encrypted\" with the SAME hidden ChaCha20-Poly1305 key AND the same reused nonce)",
            DefaultInput = "TRANSFER 100 TO BOB ACCOUNT NOW",
            DefaultKey = "MEETING MOVED TO THREE PM TODAY",
            Transform = (p1, p2) =>
            {
                var b1 = System.Text.Encoding.UTF8.GetBytes(p1);
                var b2 = System.Text.Encoding.UTF8.GetBytes(p2);
                var c1 = ModernCrypto.ChaCha20Poly1305Encrypt(b1, ChaChaDemoKey, ChaChaDemoNonce, out _);
                var c2 = ModernCrypto.ChaCha20Poly1305Encrypt(b2, ChaChaDemoKey, ChaChaDemoNonce, out _);
                var len = Math.Min(c1.Length, c2.Length);
                var xorOfCiphertexts = ClassicalCiphers.Xor(c1[..len], c2[..len]);
                var xorOfPlaintexts = ClassicalCiphers.Xor(b1[..len], b2[..len]);
                var cHex = Convert.ToHexString(xorOfCiphertexts).ToLowerInvariant();
                var pHex = Convert.ToHexString(xorOfPlaintexts).ToLowerInvariant();
                return $"C1 (hex):              {Convert.ToHexString(c1).ToLowerInvariant()}\n" +
                       $"C2 (hex):              {Convert.ToHexString(c2).ToLowerInvariant()}\n" +
                       $"C1 XOR C2:             {cHex}\n" +
                       $"P1 XOR P2 (for proof): {pHex}\n" +
                       $"Identical? {(cHex == pHex ? "YES — the exact same key-reuse failure as Lessons 9 and 12, just with ChaCha20-Poly1305 this time" : "no")}";
            },
            Explain = (_, _) =>
                "Different cipher, different internal math, same catastrophic failure the instant a " +
                "nonce gets reused — because the failure was never about AES or ChaCha20 " +
                "specifically, it's about what happens to ANY keystream-based cipher the moment the " +
                "same keystream gets used twice. Pick GCM or ChaCha20-Poly1305 based on your " +
                "hardware and ecosystem; the \"never reuse a nonce\" rule isn't optional with either " +
                "one."
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
            Transform = (input, _) =>
            {
                var repeated = string.Concat(Enumerable.Repeat(input.Length > 0 ? input : "x", 500));
                var shortHash = ModernCrypto.Sha256Hex(input);
                var longHash = ModernCrypto.Sha256Hex(repeated);
                return $"SHA-256 of your {input.Length}-character input:              {shortHash}  ({shortHash.Length} hex chars = 32 bytes)\n" +
                       $"SHA-256 of that SAME text repeated 500x ({repeated.Length,6} chars): {longHash}  ({longHash.Length} hex chars = 32 bytes)\n" +
                       $"MD5 of your input (an older, shorter algorithm): {ModernCrypto.Md5Hex(input)}  ({ModernCrypto.Md5Hex(input).Length} hex chars = 16 bytes)";
            },
            Explain = (input, _) =>
                $"\"Fixed-size output, any input size\" isn't an abstract claim — look at the two SHA-" +
                $"256 lines above: a {input.Length}-character input and a {Math.Max(input.Length, 1) * 500}-character " +
                "input (500 times longer) both produced EXACTLY 64 hex characters. Feed it one " +
                "letter, or an entire novel, and SHA-256 always hands back the same 32 bytes. Type " +
                "the exact same text again and you'll get the exact same hash — every time, forever, " +
                "no key involved. There's genuinely no operation that turns this hash back into your " +
                "original text — the only way to connect a hash back to a plaintext is to hash a " +
                "GUESS and see if it matches, which is exactly the next two lessons."
        }
    };

    // ---------------------------------------------------------------- 13b. Broken hashes

    private static TutorialStep Step13bBrokenHashes() => new()
    {
        Title = "When \"a hash\" isn't enough: MD5 and SHA-1 are broken",
        Paragraphs = new[]
        {
            "You'll still see MD5 and SHA-1 everywhere in older code, and both are called " +
            "\"broken\" — but broken does NOT mean someone figured out how to reverse them back to " +
            "the original input; that's still just as impossible as ever for any hash. Broken means " +
            "something more specific: an attacker can find TWO DIFFERENT inputs that produce the " +
            "EXACT SAME hash (a \"collision\") far faster than they should be able to. That sounds " +
            "abstract until you hit a case where it matters — a hash used to prove \"this is really " +
            "the exact file I approved\" stops proving anything if an attacker can craft a second, " +
            "malicious file sharing that same hash.",
            "Both breaks are real, documented, and happened years apart: MD5 collisions were first " +
            "demonstrated in 2004 (Wang, Feng, Lai, and Yu). SHA-1 collisions were demonstrated for " +
            "real in 2017, by Google and CWI Amsterdam (\"SHAttered\") — two genuinely different PDF " +
            "files that hash to the identical SHA-1 value. Neither algorithm got slower or changed " +
            "its output size; the guarantee that broke was \"you can't find two inputs that collide " +
            "faster than brute force.\"",
            "Here's the real mathematical reason collisions become findable AT ALL, demonstrated " +
            "live rather than just asserted: it's called the \"birthday paradox\" (the same math " +
            "behind \"in a room of just 23 people, there's better than even odds two share a " +
            "birthday\" — far fewer people than the 366 you'd intuitively guess). Finding ANY two " +
            "inputs that collide needs only roughly the SQUARE ROOT of the total number of possible " +
            "hash values, not half of them. A real full-size hash's output space is so enormous that " +
            "even a square root is still astronomically large — but the demo below shrinks the " +
            "hash down to just a few bits, small enough to brute-force for real, right now, so you " +
            "can watch the exact math play out instead of taking it on faith.",
            "This is also exactly why hash SIZE matters so much for collision resistance " +
            "specifically: MD5's 128-bit output only gives roughly 2^64 worth of real collision " +
            "resistance (the square root of 2^128) — large in 2004, not large enough against " +
            "today's hardware combined with the clever mathematical shortcuts Wang's team found. " +
            "SHA-256's 256-bit output gives roughly 2^128 worth of collision resistance instead — " +
            "comfortably out of reach of anything on the horizon, which is exactly why it replaced " +
            "both broken algorithms."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Truncate SHA-256's output to this many bits before searching for a collision (try 8-20)",
            DefaultInput = "16",
            Transform = (input, _) =>
            {
                if (!int.TryParse(input, out var bits) || bits < 4 || bits > 22)
                    return "Enter a whole number from 4 to 22 (higher takes longer to search).";

                var byteCount = (bits + 7) / 8;
                var mask = bits % 8 == 0 ? 0xFF : (0xFF << (8 - bits % 8)) & 0xFF;
                var seen = new Dictionary<string, string>();
                var maxTries = Math.Min(4_000_000, 1 << (bits + 4));
                var rng = new Random(12345);

                for (var i = 0; i < maxTries; i++)
                {
                    var candidate = $"msg-{i}-{rng.Next()}";
                    var fullHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(candidate));
                    var truncated = (byte[])fullHash[..byteCount].Clone();
                    truncated[^1] = (byte)(truncated[^1] & mask);
                    var key = Convert.ToHexString(truncated);

                    if (seen.TryGetValue(key, out var earlier))
                    {
                        var expected = Math.Sqrt(Math.Pow(2, bits));
                        return $"Collision found after {i + 1} tries:\n" +
                               $"  \"{earlier}\"\n" +
                               $"  \"{candidate}\"\n" +
                               $"both hash to {key} once truncated to {bits} bits.\n\n" +
                               $"sqrt(2^{bits}) = {expected:N0}  <- the rough order of magnitude this should take, and it did.";
                    }
                    seen[key] = candidate;
                }
                return $"No collision found in {maxTries:N0} tries — try a smaller bit count (the search is capped so this demo doesn't hang).";
            },
            Explain = (input, _) =>
                "Every single hash above was a REAL, fully computed SHA-256 hash — just truncated " +
                "down to a tiny number of bits so a collision is findable in a fraction of a second " +
                "instead of requiring a planet's worth of computers. The number of tries it actually " +
                "took should land close to the square root of 2^bits, not anywhere near 2^bits " +
                "itself — that gap between \"square root\" and \"the whole space\" is the entire " +
                "reason hash size has to be roughly DOUBLE what you'd naively expect you need, for " +
                "any hash meant to resist collisions."
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

    // ---------------------------------------------------------------- 14b. Pepper

    private static TutorialStep Step14bPepper() => new()
    {
        Title = "A second secret, kept somewhere else entirely: pepper",
        Paragraphs = new[]
        {
            "Salt stops an attacker from attacking ALL your users at once with one precomputed " +
            "table — but it does nothing to slow down attacking ONE individual password. If an " +
            "attacker steals your whole database (hash AND salt sit right next to each other, by " +
            "design — salt was never secret), they can still brute-force any single salted hash at " +
            "full speed, same as before. Salt defeats batch attacks; it was never meant to defeat an " +
            "attacker focused on one target.",
            "A pepper is a second secret value, but it works completely differently from salt in two " +
            "specific ways: it's the SAME for every single user (one value for the whole app, not " +
            "one per password), and it is NEVER stored anywhere near the database — it lives " +
            "somewhere else entirely: an environment variable, a config file outside the web root, a " +
            "dedicated secrets manager. The hash becomes hash(pepper + salt + password). An attacker " +
            "who steals ONLY the database — the single most common real-world breach, via a backup " +
            "leak or a SQL injection dump — is now missing a piece of the puzzle that was never in " +
            "what they stole, no matter how good their password guess is.",
            "One honest limit, worth stating plainly: pepper only helps when the database leak and " +
            "the pepper's storage are genuinely separate failures. If an attacker gets full control " +
            "of the server itself (not just a DB dump), they can likely read wherever the pepper " +
            "lives too, since the running application has to read it in order to check logins at " +
            "all. Pepper raises the bar specifically against \"the database leaked, but nothing " +
            "else did\" — a very common real scenario, just not every scenario."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Password",
            DefaultInput = "hunter2",
            Transform = (input, _) =>
            {
                const string realPepper = "app-wide-secret-2024"; // never stored in the DB — lives in config/secrets manager only
                var salt = Convert.ToHexString(ModernCrypto.RandomSalt(4)).ToLowerInvariant();
                var realHash = ModernCrypto.Sha256Hex(salt + realPepper + input);
                var stolenDbGuessNoPepper = ModernCrypto.Sha256Hex(salt + input);
                var stolenDbGuessWrongPepper = ModernCrypto.Sha256Hex(salt + "attackers-wrong-guess" + input);

                return $"Real stored hash (uses salt + pepper + password):  {realHash}\n\n" +
                       $"Attacker steals ONLY the database (salt + hash). They guess your EXACT real " +
                       $"password \"{input}\" correctly, but don't know a pepper exists:\n" +
                       $"  guess without any pepper:      {stolenDbGuessNoPepper}   <- {(stolenDbGuessNoPepper == realHash ? "matches (no pepper in use)" : "does NOT match")}\n" +
                       $"Attacker learns pepper exists but has to guess its value too:\n" +
                       $"  guess with the WRONG pepper:   {stolenDbGuessWrongPepper}   <- {(stolenDbGuessWrongPepper == realHash ? "matches" : "does NOT match")}";
            },
            Explain = (input, _) =>
                "Notice the attacker had your EXACT real password both times and still couldn't " +
                "produce a matching hash — because the database leak never contained the pepper, " +
                "and a correct password guess alone isn't enough anymore. Salt (Lesson 14) stopped " +
                "attacking every user at once with one table; pepper stops a stolen database from " +
                "being enough on its own, even one password at a time."
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
            Transform = (input, key) =>
            {
                if (string.IsNullOrEmpty(key)) return "Enter a key.";
                if (input.Length == 0) return "Enter a message.";

                var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
                var original = System.Text.Encoding.UTF8.GetBytes(input);
                var mutated = (byte[])original.Clone();
                var originalLastByte = mutated[^1];
                mutated[^1] ^= 0x01; // flip exactly ONE bit of the last byte — nothing else changes

                using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
                var tagOriginal = hmac.ComputeHash(original);
                var tagMutated = hmac.ComputeHash(mutated);

                var differingBits = 0;
                for (var i = 0; i < tagOriginal.Length; i++)
                {
                    var diff = (byte)(tagOriginal[i] ^ tagMutated[i]);
                    while (diff != 0) { differingBits += diff & 1; diff >>= 1; }
                }
                var totalBits = tagOriginal.Length * 8;

                return $"Message HMAC:                    {Convert.ToHexString(tagOriginal).ToLowerInvariant()}\n" +
                       $"Same message, last byte {originalLastByte:x2}->{mutated[^1]:x2} " +
                       $"(one bit flipped), HMAC: {Convert.ToHexString(tagMutated).ToLowerInvariant()}\n" +
                       $"Bits that changed: {differingBits} of {totalBits} ({(double)differingBits / totalBits:P0}) " +
                       $"— from flipping exactly 1 of the {original.Length * 8} input bits.";
            },
            Explain = (input, key) =>
                "That one-bit change (not even a whole character — just the message's very last " +
                "bit) still flipped roughly HALF of the 256 output bits, with no visible pattern " +
                "connecting which input bit moved to which output bits changed. That's the " +
                "\"avalanche effect\": every bit of a cryptographic hash/HMAC output is meant to " +
                "depend on every bit of the input, so no attacker can predict \"if I change this " +
                "part, that part of the output moves predictably.\" Try editing the message or key " +
                "yourself above and recompute — you'll see the same near-50% churn every time, " +
                "regardless of how small your edit was."
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
            "Modern authenticated modes like GCM (Lesson 12) close this off by design.",
            "The demo below doesn't just describe the attack, it RUNS it — a real \"oracle\" " +
            "function that genuinely AES-decrypts a tampered ciphertext and reports only pass/fail " +
            "on the padding (exactly what a vulnerable server would leak), and a real attacker-side " +
            "loop that queries it up to 256 times per byte, working backward from the last byte of " +
            "the final block, to recover the entire block — without the attack code ever touching " +
            "the key. Type your own message (16+ characters) and watch it recover your own last " +
            "block, one byte at a time."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Secret message (16+ characters so there's a full block to attack)",
            DefaultInput = "THE LAUNCH CODE IS 4217",
            Transform = (input, _) => PaddingOracleAttack(input),
            Explain = (_, _) =>
                "Each row above is one real byte recovered purely from oracle yes/no answers: the " +
                "attack guesses a tampered byte value (0-255), the oracle genuinely decrypts and " +
                "checks PKCS7 padding, and the FIRST guess that validates reveals one byte of the " +
                "real plaintext through pure XOR arithmetic — no key required. (One subtlety baked " +
                "into the code: the very first byte guessed has to be double-checked against a " +
                "second, unrelated tamper, because a 9-byte padding can accidentally look like a " +
                "valid 1-byte padding — the real attack handles this, and so does this demo.) If " +
                "your message's length is a multiple of 16, PKCS7 adds one whole extra block of pure " +
                "padding — the recovered \"message\" will just be padding bytes; try a different " +
                "length to recover real characters instead."
        }
    };

    /// <summary>Runs a REAL CBC padding-oracle attack against a message encrypted with real AES:
    /// an "oracle" that genuinely decrypts+validates PKCS7 padding and nothing else (exactly what a
    /// vulnerable server leaks), queried by a real byte-by-byte recovery loop that never touches the
    /// key. Algorithm and the pos==15 disambiguation step verified against .NET's own AES decryption
    /// (and against the known plaintext) via an independent PowerShell reimplementation before use.</summary>
    private static string PaddingOracleAttack(string input)
    {
        var plaintext = System.Text.Encoding.UTF8.GetBytes(input);
        if (plaintext.Length < 16) return "Type a message of at least 16 characters so there's a full block to attack.";

        var key = ModernCrypto.DemoKeyFromPassphrase("tutorial-demo-key");
        var iv = ModernCrypto.ZeroIv16;
        var ciphertext = ModernCrypto.AesCbcEncrypt(plaintext, key, iv);

        var blockCount = ciphertext.Length / 16;
        var prevStart = (blockCount - 2) * 16;
        var targetStart = (blockCount - 1) * 16;
        var prevOriginal = ciphertext[prevStart..(prevStart + 16)];
        var targetBlock = ciphertext[targetStart..(targetStart + 16)];
        var preceding = ciphertext[..prevStart];

        bool OracleAccepts(byte[] tamperedPrev)
        {
            var trial = new byte[preceding.Length + 32];
            preceding.CopyTo(trial, 0);
            tamperedPrev.CopyTo(trial, preceding.Length);
            targetBlock.CopyTo(trial, preceding.Length + 16);

            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            using var decryptor = aes.CreateDecryptor();
            try { decryptor.TransformFinalBlock(trial, 0, trial.Length); return true; }
            catch (System.Security.Cryptography.CryptographicException) { return false; }
        }

        var intermediate = new byte[16];
        var log = new List<string>();
        for (var pos = 15; pos >= 0; pos--)
        {
            var padVal = (byte)(16 - pos);
            var tamperedPrev = (byte[])prevOriginal.Clone();
            for (var j = pos + 1; j <= 15; j++) tamperedPrev[j] = (byte)(intermediate[j] ^ padVal);

            var foundGuess = -1;
            for (var guess = 0; guess <= 255; guess++)
            {
                tamperedPrev[pos] = (byte)guess;
                if (!OracleAccepts(tamperedPrev)) continue;
                if (pos == 15)
                {
                    var disambiguate = (byte[])tamperedPrev.Clone();
                    disambiguate[14] ^= 0xFF;
                    if (!OracleAccepts(disambiguate)) continue; // coincidental hit, not the real padVal=1 case
                }
                foundGuess = guess;
                break;
            }
            if (foundGuess < 0) return "The oracle never validated at byte position " + pos + " — try a different message.";

            intermediate[pos] = (byte)(foundGuess ^ padVal);
            var recoveredByte = (byte)(intermediate[pos] ^ prevOriginal[pos]);
            var printable = recoveredByte is >= 0x20 and < 0x7F ? ((char)recoveredByte).ToString() : $"0x{recoveredByte:x2}";
            log.Add($"byte {pos,2}: tried guesses until 0x{foundGuess:x2} validated -> recovered '{printable}'");
        }

        var recoveredBlock = new byte[16];
        for (var i = 0; i < 16; i++) recoveredBlock[i] = (byte)(intermediate[i] ^ prevOriginal[i]);

        var padLen = recoveredBlock[15];
        var readable = padLen is >= 1 and <= 16
            ? System.Text.Encoding.UTF8.GetString(recoveredBlock, 0, 16 - padLen)
            : System.Text.Encoding.UTF8.GetString(recoveredBlock);

        return string.Join("\n", log) +
               $"\n\nFull recovered block (hex): {Convert.ToHexString(recoveredBlock).ToLowerInvariant()}" +
               $"\nRecovered text (padding stripped): \"{readable}\"";
    }

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
            Explain = (aSecretStr, bSecretStr) =>
            {
                if (!int.TryParse(aSecretStr, out var aSecret)) return "Enter a whole number for Alice's secret.";
                const int p = 23, g = 5;
                var aPublic = ModPow(g, aSecret, p);

                var tries = 0;
                var recoveredSecret = -1;
                for (var guess = 1; guess < p; guess++)
                {
                    tries++;
                    if (ModPow(g, guess, p) == aPublic) { recoveredSecret = guess; break; }
                }

                var matchNote = recoveredSecret == aSecret
                    ? $"exactly Alice's real secret"
                    : "a DIFFERENT secret that happens to produce the identical public value — p=23 " +
                      "is so small that more than one secret can collide this way, which essentially " +
                      "never happens at real-world sizes";

                return "An eavesdropper watching p, g, and Alice's PUBLIC value doesn't have to " +
                       "out-think the math — at THIS tiny scale they can just try every possible " +
                       "secret number and check which one reproduces Alice's public value, exactly " +
                       "like trying every Caesar shift back in Lesson 2:\n\n" +
                       $"  Alice's public value: {aPublic}\n" +
                       $"  Brute-force search: try secret=1,2,3,...  compute g^secret mod {p}  stop when it equals {aPublic}\n" +
                       $"  Found it at secret={recoveredSecret}, after just {tries} guess(es) — {matchNote}.\n\n" +
                       $"That search succeeded almost instantly ONLY because p={p} is absurdly small. " +
                       "Real systems use primes hundreds of digits long, where this exact same " +
                       "brute-force search would need vastly more attempts than there are atoms in " +
                       "the observable universe — reversing the modular exponentiation is believed to " +
                       "be computationally infeasible at that scale, which is the entire reason " +
                       "Diffie-Hellman, scaled up, is what lets your browser and a web server agree " +
                       "on a session key before they've ever communicated before.";
            }
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

    // ---------------------------------------------------------------- 18b. Public/private keys (RSA)

    private static readonly Lazy<System.Security.Cryptography.RSA> RealRsaDemoKey =
        new(() => System.Security.Cryptography.RSA.Create(2048));

    private static TutorialStep Step18bPublicPrivateKeys() => new()
    {
        Title = "A key that can't undo its own lock: public and private keys",
        Paragraphs = new[]
        {
            "## The problem this solves",
            "Diffie-Hellman (last lesson) solves ONE specific problem: two people who both show up " +
            "at the same time can agree on a shared secret in front of an eavesdropper. But what if " +
            "someone wants to send YOU something secret without either of you being online at the " +
            "same moment at all — say, encrypting a file to leave for you, using nothing but a key " +
            "you posted publicly somewhere, with you reading it hours or days later? That needs a " +
            "completely different kind of key — one that comes in two parts that do different jobs, " +
            "instead of one shared secret both sides hold equally.",

            "## The scene, before any math at all",
            "You want to be able to receive secret notes from anyone — a friend, a stranger, anyone " +
            "— sent through the mail, where anybody along the way could grab the package and look " +
            "inside.",
            "So you buy a padlock. Here's the one fact that makes this whole thing work, and it's " +
            "true of a REAL physical padlock, no trick involved: locking it needs no key at all. You " +
            "just push the shackle in until it clicks shut. Anyone can do that, with zero " +
            "information. Only opening it needs the key — one specific physical key, cut to match " +
            "that one padlock.",
            "You take that key and put it on a chain around your own neck. It never comes off, never " +
            "gets copied, never gets mailed anywhere. (In Windows, you can go even further — a " +
            "certificate's private key can be flagged non-exportable right in the OS, so there's no " +
            "copy command that ever gets it off that one machine. That flag applies per INSTALLED " +
            "COPY, not to the key pair itself: you can still put the same key on several machines by " +
            "exporting one master copy first, then marking each installed copy non-exportable " +
            "afterward. The only way to truly have just one copy anywhere is to generate it directly " +
            "in a non-exportable or hardware-bound context — a TPM or HSM — where no exportable file " +
            "is ever produced at all, and losing that one machine really does lose it for good.)",
            "The padlock itself — unlocked, open, ready to snap shut — you make as many copies of as " +
            "you want, and hand them out to absolutely anyone who asks. Your friend. A stranger. " +
            "Someone you don't even trust. It doesn't matter, because an open padlock sitting in " +
            "someone's hand can't do anything dangerous by itself — all it can do is get snapped " +
            "shut, and snapping it shut is the one thing that needs no key.",
            "Your friend wants to send you a note. They write it, put it in a box, and snap one of " +
            "your padlocks shut on the box. Notice what just happened: they needed no key to do " +
            "that. And notice something else — your friend can no longer open that box either. They " +
            "just locked themselves out of their own note, using nothing but a padlock you handed " +
            "out for free.",
            "They mail the box. Anyone who intercepts it along the way sees a locked box and is in " +
            "the exact same position your friend is: they can push the shackle, sure, but they can't " +
            "pull it open, because they don't have the one key on the chain around your neck.",
            "The box arrives. You take the one key only you have, unlock it, and read the note.",

            "## Mapping the scene to the digital version",
            "- The padlock you hand out to anyone -> your PUBLIC key. Handing it to your worst " +
            "enemy is fine — it was never a secret, and was never meant to be one.\n" +
            "- Snapping the padlock shut (no key needed) -> ENCRYPTING with the public key — " +
            "running the message through the public key's numbers.\n" +
            "- The one key on the chain around your neck -> your PRIVATE key.\n" +
            "- Only that one key opening that one padlock -> only the PRIVATE key's numbers can " +
            "reverse what the public key's numbers did. Not \"won't\" — can't. The information " +
            "needed to reverse it was never in the public copy to begin with.\n" +
            "- Anyone being able to have a copy of the open padlock -> public keys are meant to be " +
            "given to literally anyone, including people you don't trust.",

            "## A building block: what's a \"totient\"",
            "The real numbers below need one building-block number first: the TOTIENT of a number " +
            "n (written φ(n), \"Euler's totient,\" 1760s) is just a count — how many whole numbers " +
            "from 1 up to n share NO common factor with n? Small example first: n=10. The numbers " +
            "1-9 are 1,2,3,4,5,6,7,8,9. Cross out anything sharing a factor with 10 (10=2x5): 2, 4, " +
            "6, 8 all share a factor of 2; 5 shares a factor of 5. What's left: 1, 3, 7, 9 — four " +
            "numbers. So φ(10)=4.",
            "Handy shortcut when n is two primes multiplied together: (2-1)x(5-1) = 1x4 = 4 — same " +
            "answer, no counting needed. That shortcut is why φ(55) = (5-1)x(11-1) = 4x10 = 40 " +
            "further down, without checking all 54 numbers by hand (it was checked by computer " +
            "before writing this, and the brute-force count really is 40).",
            "Why does this counting exercise matter for encryption at all? Because of a real, " +
            "proven mathematical fact (Euler's theorem): raise ANY number m that shares no factor " +
            "with n up to the power of φ(n), then take the result mod n, and you always land on " +
            "exactly 1 — no matter what m is. Check it concretely: m=3, n=10, φ(10)=4 -> 3^4 mod 10 " +
            "= 81 mod 10 = 1. Exactly 1. That single fact is the entire reason picking e and d so " +
            "that (e x d) mod φ(n) = 1 makes \"encrypt then decrypt\" land you back on the original " +
            "message — e x d works out to some multiple of φ(n) plus exactly 1 left over, and that " +
            "leftover +1 is the only part that survives once everything else cancels out via " +
            "Euler's theorem.",

            "## The real key generation",
            "Pick two small primes: p=5, q=11. Multiply: n = 5 x 11 = 55 — this number ends up in " +
            "BOTH keys, it's the \"size of the padlock,\" shared, not secret by itself. Compute the " +
            "totient: (5-1)x(11-1) = 40 (used only to build the keys, then it must never be shared " +
            "again — see the attack below for exactly why).",
            "Pick a small public number e=7. Find a number d where (e x d) mod 40 = 1 — that number " +
            "is d=23, because 7 x 23 = 161, and 161 mod 40 = 1.",
            "  Public key (the padlock):      n=55, e=7\n" +
            "  Private key (the chain key):   n=55, d=23",
            "Both have 55. The ENTIRE difference between \"can only lock\" and \"can unlock\" is one " +
            "number: e=7 vs. d=23.",

            "## Locking and unlocking, for real",
            "Snapping the padlock shut, for real: message = 9. Encrypt with the public pair: " +
            "9^7 mod 55 = 4. Send 4.",
            "Opening it with the one real key: decrypt with the private pair: 4^23 mod 55 = 9. Back " +
            "to the original message.",
            "Proof the public key really can't open it: try \"decrypting\" that same 4 using the " +
            "PUBLIC number instead of the private one — 4^7 mod 55 = 49. Not 9. Garbage — because " +
            "the public key never contained 23 anywhere, so there's nothing in it capable of " +
            "reversing the lock.",
            "One more, so it repeats: message = 3. Public pair encrypts: 3^7 mod 55 = 42. Private " +
            "pair decrypts: 42^23 mod 55 = 3. Same padlock, same chain key, every time.",

            "## Why real keys need enormous primes",
            "Here's exactly what survives after key generation and what doesn't, and why that split " +
            "is safe: n is published, part of both keys. p, q, and the totient (40) must never be " +
            "shared — not because they're \"used up,\" but because they're AS dangerous as d itself. " +
            "Watch what happens when an attacker who has ONLY the public key (n=55, e=7 — nothing " +
            "else) tries to rebuild the private key from scratch:",
            "  Step 1 - factor n:        55 = 5 x 11          (found instantly, just try 2,3,4,5...)\n" +
            "  Step 2 - rebuild totient: (5-1) x (11-1) = 40\n" +
            "  Step 3 - rebuild d:       same search as before -> d=23\n" +
            "  Step 4 - decrypt the\n" +
            "           stolen ciphertext (4):  4^23 mod 55 = 9   <- fully cracked, no private key ever handed over",
            "That took a computer a fraction of a millisecond, because factoring 55 means just " +
            "trying small numbers until one divides evenly. This is the ENTIRE reason real RSA keys " +
            "use enormous primes — not 1-2 digits like this demo, but roughly 300 digits each, " +
            "making n around 600 digits long. The math you just watched happen IS exactly the math " +
            "real RSA runs — multiply two primes, same formulas, same steps — only the size " +
            "changes. Factoring a 600-digit number back into its two primes is believed to need far " +
            "more computing power than humanity has ever built; the current real-world record for " +
            "factoring a number anywhere near that range (\"RSA-250,\" a 250-digit number) took " +
            "roughly the equivalent of 2,700 years of one computer running nonstop — and a real " +
            "2048-bit key's modulus is still dramatically larger than that record."
        },
        Demo = new CipherDemo
        {
            InputLabel = "A message to encrypt with a REAL 2048-bit RSA key pair",
            DefaultInput = "meet me at midnight",
            Transform = (input, _) =>
            {
                var rsa = RealRsaDemoKey.Value;
                var publicKeyBase64 = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
                var exponentBytes = rsa.ExportParameters(false).Exponent!;
                var exponentValue = exponentBytes.Aggregate(0L, (acc, b) => (acc << 8) | b);

                var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(input);
                var ciphertext = rsa.Encrypt(plaintextBytes, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
                var ciphertextBase64 = Convert.ToBase64String(ciphertext);
                var decryptedText = System.Text.Encoding.UTF8.GetString(
                    rsa.Decrypt(ciphertext, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256));

                var approxDigits = (int)Math.Round(rsa.KeySize * Math.Log10(2));

                return $"Public key modulus size: {rsa.KeySize} bits (~{approxDigits} decimal digits — our toy n=55 was 2 digits)\n" +
                       $"Public exponent (e): {exponentValue}  <- same role as e=7 above, just a fixed real-world choice\n" +
                       $"Public key file (base64, truncated): {publicKeyBase64[..Math.Min(60, publicKeyBase64.Length)]}... ({publicKeyBase64.Length} characters total)\n\n" +
                       $"Your message: \"{input}\" ({plaintextBytes.Length} bytes)\n" +
                       $"Encrypted (base64, truncated): {ciphertextBase64[..Math.Min(60, ciphertextBase64.Length)]}... ({ciphertextBase64.Length} characters, always {ciphertext.Length} bytes for this key size)\n" +
                       $"Decrypted with the real private key: \"{decryptedText}\"\n" +
                       $"Match: {(decryptedText == input ? "YES" : "no")}";
            },
            Explain = (_, _) =>
                "Same operation as the toy example above — encrypt with the public pair, decrypt " +
                "with the private pair — just with numbers around 600 digits long instead of 2, " +
                "which is why the ciphertext is a long unreadable block instead of a single small " +
                "number. Run this again (or retype the exact same message) and the ciphertext comes " +
                "out COMPLETELY different each time, even though the message didn't change — real " +
                "RSA mixes in random padding bytes before encrypting (called OAEP, a different kind " +
                "of \"padding\" than the block-alignment padding from Lesson 11/17 — same word, " +
                "unrelated problem: without this randomness, the exact same message would always " +
                "produce the exact same ciphertext, which leaks information on its own). The public " +
                "exponent is a real, extremely common industry choice (65537) — not because it's " +
                "mathematically special, but because in binary it's 10000000000000001 (only two " +
                "1-bits), which makes the repeated-squaring math this tutorial has used since the " +
                "Diffie-Hellman lesson run faster."
        }
    };

    // ---------------------------------------------------------------- 18c. Digital signatures

    private static TutorialStep Step18cDigitalSignatures() => new()
    {
        Title = "Running the padlock backward: digital signatures",
        Paragraphs = new[]
        {
            "Last lesson's padlock locks with the public key and unlocks with the private key — " +
            "that's ENCRYPTION, built to keep something secret. Signing solves a different problem " +
            "entirely (proving a message really came from you, untampered) and runs the exact same " +
            "two keys in the OPPOSITE direction. Here's the scene, play it out fully before any math.",
            "You have a signet ring — a metal stamp, cut with a pattern unique to you. Only you have " +
            "this ring; it never leaves your hand. You write a document, melt a blob of wax onto it, " +
            "and press your ring into the wax. Anyone who has ever seen an IMPRESSION of your ring " +
            "before (not the ring itself — just what its stamp looks like, which you've shown " +
            "around freely) can look at this new wax seal and confirm: yes, that pattern matches, " +
            "this really was stamped by that specific ring. What they CANNOT do is press a new, " +
            "valid seal themselves — knowing what your stamp's pattern looks like doesn't let you " +
            "reproduce the ring that makes it.",
            "Mapping that to the digital version: your PRIVATE key presses the seal (signs) — the " +
            "one thing only you can do, since only you hold it. Your PUBLIC key checks a seal " +
            "against the known pattern (verifies) — anyone can do this, freely, forever, without " +
            "ever being able to forge a new one. Notice this is the REVERSE of encryption's roles: " +
            "there, the public key locked and only the private key could undo it; here, the private " +
            "key signs and the public key merely checks.",
            "One more piece before the math: you don't press your seal onto the entire document " +
            "directly — a real RSA signature operation is slow enough that signing a huge file " +
            "letter-by-letter would be impractical. Instead, you hash the document first (Lesson " +
            "13's fixed-size fingerprint, the same SHA-256 used throughout this tutorial) and sign " +
            "THAT. Verifying means the other person re-hashes the document themselves and checks " +
            "your signature against that fresh hash — so if even one bit of the document changed " +
            "after signing, the hash changes completely (the avalanche effect from Lesson 16's HMAC " +
            "demo applies here too, same property, different algorithm), and the old signature no " +
            "longer matches anything.",
            "## The real math",
            "RSA again, just run with the exponents in the opposite order from encryption. Reusing " +
            "the exact same toy key pair from last lesson (n=55, e=7, d=23) and pretending a " +
            "document hashes down to the small number 9:",
            "  Sign (private key first):   9^23 mod 55 = 14   <- the \"signature\"\n" +
            "  Verify (public key second):  14^7 mod 55 = 9    <- matches the original hash, signature is valid",
            "And the tamper check: if the document had been altered so its hash came out as 10 " +
            "instead of 9, the verifier would compare 9 (what the old signature actually proves) " +
            "against 10 (what the document NOW hashes to) — they don't match, so verification fails, " +
            "exactly as it should. One more pair, so it repeats: hash=3 signs to 3^23 mod 55 = 27, " +
            "and verifies back with 27^7 mod 55 = 3.",
            "The live demo below signs and verifies real text with a real 2048-bit RSA key pair, " +
            "using .NET's actual signing APIs — and then deliberately tampers with one character to " +
            "show verification fail for real, not narrated."
        },
        Demo = new CipherDemo
        {
            InputLabel = "A message to sign",
            DefaultInput = "I, the real author, approve this release.",
            Transform = (input, _) =>
            {
                var rsa = RealRsaDemoKey.Value;
                var data = System.Text.Encoding.UTF8.GetBytes(input);
                var signature = rsa.SignData(data, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pss);
                var signatureBase64 = Convert.ToBase64String(signature);

                var originalValid = rsa.VerifyData(data, signature, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pss);

                var tampered = input.Length > 0
                    ? input[..^1] + (char)(input[^1] + 1)
                    : input;
                var tamperedData = System.Text.Encoding.UTF8.GetBytes(tampered);
                var tamperedValid = rsa.VerifyData(tamperedData, signature, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pss);

                return $"Signature (base64, truncated): {signatureBase64[..Math.Min(60, signatureBase64.Length)]}... ({signature.Length} bytes)\n\n" +
                       $"Verify against the ORIGINAL message: {(originalValid ? "VALID" : "invalid")}\n" +
                       $"Verify that SAME signature against a one-character-tampered message\n" +
                       $"  (\"{tampered}\"): {(tamperedValid ? "valid" : "INVALID — tampering detected")}";
            },
            Explain = (_, _) =>
                "Same signature bytes, checked against two messages that differ by exactly one " +
                "character — valid against the real one, flatly rejected against the tampered one. " +
                "That's the hash's avalanche effect doing the real work: changing one character " +
                "changes the hash completely, and the signature only ever matches one specific " +
                "hash. Run this again and the signature bytes themselves come out different each " +
                "time too (RSASSA-PSS, the modern signature padding, mixes in random salt the same " +
                "way OAEP did for encryption last lesson) — yet verification still succeeds every " +
                "time against the untampered message, because validity never depended on the " +
                "signature being identical run to run, only on it matching that one hash."
        }
    };

    // ---------------------------------------------------------------- 18d. Certificates and PKI

    private static TutorialStep Step18dCertificatesAndPki() => new()
    {
        Title = "A notarized ID card for a public key: certificates",
        Paragraphs = new[]
        {
            "You now have everything a certificate actually is built from. A certificate is a " +
            "bundle of plain fields — who it belongs to (Subject), who is vouching for it (Issuer), " +
            "the subject's PUBLIC key, a validity date range, a few other identifiers — plus a " +
            "SIGNATURE over the hash of that whole bundle, made with the ISSUER's private key. " +
            "\"Signing a certificate\" is not a different operation from last lesson — it's the " +
            "identical hash-then-sign process, just applied to a structured set of fields instead " +
            "of a plain sentence.",
            "Verifying one works exactly like verifying any signature: recompute the hash of the " +
            "cert's fields yourself, check the signature against it using the ISSUER's public key. " +
            "If it checks out, you now trust this stranger's public key — not because you ever met " +
            "them, but purely because a party you ALREADY trusted vouched for it. That's the entire " +
            "trick. No magic, same math as the wax seal.",
            "Chain of trust: that issuer's own certificate was ALSO signed by someone (or, for a " +
            "\"root\" CA, by itself). Walk up the chain of \"who signed whose certificate\" and you " +
            "eventually reach one of a small number of ROOT certificates that are pre-installed in " +
            "your operating system or browser, trusted by fiat — someone decided to trust them, " +
            "continuously audits them, and ships their public keys with every copy of Windows, " +
            "macOS, Chrome, and so on.",
            "What actually makes a CA an authority — concretely, since \"they just are\" isn't a real " +
            "answer: there are different VALIDATION LEVELS, and they are not all the same amount of " +
            "scrutiny. Domain Validation (DV) — the vast majority of certificates issued today, " +
            "including every free Let's Encrypt cert — proves ONLY that you control the domain's DNS " +
            "or web server (put a specific value in a DNS TXT record, or host a specific file at a " +
            "specific URL). Fully automated, zero humans involved, issued in seconds. Organization " +
            "Validation (OV) and especially Extended Validation (EV) go much further: confirming the " +
            "organization's legal existence through official business registries, verifying a " +
            "physical address, and — yes, this part you heard is real — a phone call to a " +
            "verified business number, per the published CA/Browser Forum guidelines those higher " +
            "tiers require. So \"they called me\" is true, but only for the small minority of certs " +
            "that pay for that higher assurance level, not the free DV cert most small sites use."
        },
        Demo = new CipherDemo
        {
            InputLabel = "Subject name for a real, self-signed certificate",
            DefaultInput = "CN=my-test-site.local",
            Transform = (input, _) =>
            {
                try
                {
                    using var rsa = System.Security.Cryptography.RSA.Create(2048);
                    var req = new System.Security.Cryptography.X509Certificates.CertificateRequest(
                        input, rsa, System.Security.Cryptography.HashAlgorithmName.SHA256,
                        System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                    using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(365));

                    using var chain = new System.Security.Cryptography.X509Certificates.X509Chain();
                    var trusted = chain.Build(cert);
                    var statuses = string.Join(", ", chain.ChainStatus.Select(s => s.Status));

                    return $"Subject:              {cert.Subject}\n" +
                           $"Issuer:               {cert.Issuer}  <- same as Subject: nobody but itself vouches for this\n" +
                           $"Valid:                {cert.NotBefore:yyyy-MM-dd} to {cert.NotAfter:yyyy-MM-dd}\n" +
                           $"Signature algorithm:  {cert.SignatureAlgorithm.FriendlyName}\n" +
                           $"Public key size:      {rsa.KeySize} bits\n" +
                           $"Thumbprint:           {cert.Thumbprint}\n\n" +
                           $"Chain trusted by this machine's root store? {(trusted ? "YES" : "NO")}" +
                           (trusted ? "" : $"  ({(statuses.Length > 0 ? statuses : "untrusted")})");
                }
                catch (ArgumentException)
                {
                    return "Enter a valid X.509 subject name, e.g. CN=example.local";
                }
            },
            Explain = (_, _) =>
                "This is a REAL X.509 certificate, built and signed for real, with a real RSA key " +
                "pair, just like a CA would build one — except Issuer and Subject are identical, " +
                "because it signed itself; nobody else ever vouched for it. Watch the chain-trust " +
                "check: it genuinely fails (\"UntrustedRoot\"), because this machine's root store " +
                "has no reason to trust a certificate that only trusts itself. A real CA-issued " +
                "certificate passes this exact same check only because its issuer traces back, " +
                "signature by signature, to one of the roots already baked into your OS."
        }
    };

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
