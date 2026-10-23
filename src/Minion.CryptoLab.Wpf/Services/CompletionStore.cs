using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Minion.CryptoLab.Wpf.Services;

/// <summary>Which lessons (by their Number, e.g. "02", "12b") the user has visited, persisted
/// to disk so the Home screen's progress tiles survive across app runs. A lesson is marked
/// complete the moment it's opened, matching how the design's status lines ("3 of 6 complete")
/// are meant to track "seen", not a graded pass/fail.</summary>
public static class CompletionStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Minion.CryptoLab", "progress.json");

    private static readonly HashSet<string> Completed = Load();

    public static bool IsCompleted(string lessonNumber) => Completed.Contains(lessonNumber);

    public static event Action? Changed;

    public static void MarkCompleted(string lessonNumber)
    {
        if (!Completed.Add(lessonNumber)) return;
        Save();
        Changed?.Invoke();
    }

    public static void Reset()
    {
        if (Completed.Count == 0) return;
        Completed.Clear();
        Save();
        Changed?.Invoke();
    }

    private static HashSet<string> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new HashSet<string>();
            var json = File.ReadAllText(FilePath);
            var values = JsonSerializer.Deserialize<string[]>(json);
            return values is null ? new HashSet<string>() : new HashSet<string>(values);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new HashSet<string>();
        }
    }

    private static void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Completed.OrderBy(s => s).ToArray()));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // best-effort: progress just won't persist this run
        }
    }
}
