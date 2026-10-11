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
            "different Caesar shift for each letter in turn — letter 1 of the message shifts by " +
            "letter 1 of the key, letter 2 by letter 2 of the key, and so on. Once the key runs out " +
            "of letters, it just starts over from its own beginning and keeps going, for as long as " +
            "the message runs. Each key letter means \"shift by this many places,\" exactly like a " +
            "Caesar shift: A=0, B=1, C=2, ... Z=25.",
            "First, the key itself, converted to numbers the same A=0...Z=25 way — this is the " +
            "ENTIRE source of every shift below, nothing about it is random:\n\n" +
            "L=11   E=4   M=12   O=14   N=13\n\n" +
            "Here's the first example spelled out completely, letter by letter, key \"LEMON\" lined " +
            "up under message \"ATTACKATDAWN\" (the key repeats: L-E-M-O-N-L-E-M-O-N-L-E once it " +
            "runs out at letter 6):\n\n" +
            "Plaintext:  A   T   T   A   C   K   A   T   D   A   W   N\n" +
            "Key letter: L   E   M   O   N   L   E   M   O   N   L   E\n" +
            "Shift by:   11  4   12  14  13  11  4   12  14  13  11  4\n" +
            "Ciphertext: L   X   F   O   P   V   E   F   R   N   H   R\n\n" +
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
            "One more full example, different words, so the pattern is unmistakable — message " +
            "\"HELLOWORLD\" with the short 3-letter key \"KEY\" (repeating as K-E-Y-K-E-Y-K-E-Y-K, " +
            "three full times plus one letter). Key legend first: K=10, E=4, Y=24 — and that's it, " +
            "only three distinct shift values exist anywhere in this entire example:\n\n" +
            "Plaintext:  H   E   L   L   O   W   O   R   L   D\n" +
            "Key letter: K   E   Y   K   E   Y   K   E   Y   K\n" +
            "Shift by:   10  4   24  10  4   24  10  4   24  10\n" +
            "Ciphertext: R   I   J   V   S   U   Y   V   J   N\n\n" +
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
            "XOR (\"exclusive or\") takes two single bits and produces one bit back. It asks exactly " +
            "one question: ARE THE TWO INPUT BITS DIFFERENT? If yes, the result is 1. If they're the " +
            "same, the result is 0. That is the ENTIRE rule — there is nothing else to it. Written " +
            "out as every possible case (only four exist, since a bit is only ever 0 or 1):\n" +
            "\n" +
            "  Bit A   Bit B   A XOR B   (same as each other?)\n" +
            "    0       0        0      yes -> 0\n" +
            "    0       1        1      no  -> 1\n" +
            "    1       0        1      no  -> 1\n" +
            "    1       1        0      yes -> 0",
            "Now the part that makes XOR useful for a cipher: XOR-ing a result with the SAME bit " +
            "again always undoes it, for both possible keys, with no exceptions — because there are " +
            "only four cases total, we can just check every single one, by hand:\n" +
            "\n" +
            "  Plain(P)  Key(K)  Encrypt: C=P XOR K  Decrypt: C XOR K   Got P back?\n" +
            "     0        0            0                 0               yes\n" +
            "     0        1            1                 0               yes\n" +
            "     1        0            1                 1               yes\n" +
            "     1        1            0                 1               yes\n" +
            "\n" +
            "Look at the \"Decrypt\" column: it's identical to the original \"Plain\" column, every " +
            "single time, no matter what P or K were. That's not a coincidence that happens to work " +
            "for these numbers — it's EVERY possible case for a single bit, so there's nothing left " +
            "to be wrong. That is the entire mathematical proof that \"XOR the same key twice\" always " +
            "gets you back to where you started.",
            "A letter isn't one bit, though — it's a whole BYTE, 8 bits glued together, and XOR just " +
            "does the exact same single-bit operation 8 times, once per matching position, completely " +
            "independently. Take the letter 'H' and XOR it with the letter 'o' used as a one-byte " +
            "\"key\" — first, their bits (every letter has a fixed binary code; 'H' is always " +
            "01001000, 'o' is always 01101111):\n" +
            "\n" +
            "  'H' (plaintext):  0 1 0 0 1 0 0 0\n" +
            "  'o' (key):        0 1 1 0 1 1 1 1\n" +
            "  XOR, bit by bit:  0 0 1 0 0 1 1 1   <- compare each column above using the table higher up\n" +
            "\n" +
            "Read the 8 result bits left to right as one byte: 00100111. Converted from binary, that's " +
            "the number 39, which is the character ' (an apostrophe) — scrambled, unreadable as " +
            "'H', exactly what we want from encryption.",
            "Now undo it — decrypt by XOR-ing that SAME result with the SAME key again, bit by bit, " +
            "no different operation needed:\n" +
            "\n" +
            "  00100111 (ciphertext, the apostrophe):  0 0 1 0 0 1 1 1\n" +
            "  'o' (the same key again):                0 1 1 0 1 1 1 1\n" +
            "  XOR, bit by bit:                         0 1 0 0 1 0 0 0\n" +
            "\n" +
            "Read those 8 bits: 01001000 = 72 = 'H'. Back to the original letter, using the identical " +
            "XOR operation both times — nothing marked \"decrypt mode,\" nothing reversed, just XOR " +
            "with the same key a second time.",
            "One letter is a good start, but a real message is a short STRING of letters, each one " +
            "XOR-ed with its own key byte in the same position. Keeping it short on purpose: encrypt " +
            "\"HI\" (2 letters) with the 2-letter key \"ok\" — 'H' pairs with 'o' (worked out above, " +
            "giving the apostrophe), and 'I' pairs with 'k':\n" +
            "\n" +
            "  'I' (plaintext):  0 1 0 0 1 0 0 1\n" +
            "  'k' (key):        0 1 1 0 1 0 1 1\n" +
            "  XOR, bit by bit:  0 0 1 0 0 0 1 0\n" +
            "\n" +
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
