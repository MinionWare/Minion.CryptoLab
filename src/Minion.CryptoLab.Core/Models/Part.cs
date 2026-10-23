namespace Minion.CryptoLab.Core.Models;

/// <summary>Which of the four curriculum eras a lesson belongs to (or <see cref="Hub"/> for
/// screens that aren't inside any era — currently only the wrap-up).</summary>
public enum Part { Hub, I, II, III, IV }

/// <summary>The fixed facts about one part: its title, era label, and the intro copy shown on
/// its divider screen. Lesson text itself stays in TutorialContent; this is just the grouping.</summary>
public sealed record PartInfo(Part Part, string Roman, string Title, string Era, string DateRange, string Intro)
{
    public static readonly IReadOnlyList<PartInfo> All =
    [
        new(Part.I, "I", "Hand ciphers", "Antiquity", "c. 50 BC",
            "Seven lessons you could work through with a pen, a strip of paper and some patience. " +
            "Each cipher fixes the last one's weakness and introduces a new one."),
        new(Part.II, "II", "Raw bits", "The Machine Age", "1917-1970s",
            "From here on a message is a stack of bytes, and every demo runs real cryptography on " +
            "whatever you type. Part II goes from XOR to modern authenticated encryption."),
        new(Part.III, "III", "Storing secrets", "The Dial-up Age", "1990s",
            "Encryption hides data you need back. Hashing proves you know something without keeping " +
            "it. These seven lessons cover how passwords should be stored and how stored data gets tampered with."),
        new(Part.IV, "IV", "Keys & trust", "The Network Age", "2000s-today",
            "Everything so far assumed both sides already share a key. Part IV shows how strangers " +
            "agree on one in public, and how you know who you're talking to.")
    ];

    public static PartInfo Of(Part part) => All.First(p => p.Part == part);
}
