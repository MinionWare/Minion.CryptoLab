using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Minion.CryptoLab.Wpf.Services;
using Minion.Licensing.Updates;

namespace Minion.CryptoLab.Wpf;

public partial class MainWindow : Window
{
    private readonly UpdateCheckService _updateCheckService = new("Minion.CryptoLab");
    private UpdateManifest? _pendingUpdateManifest;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => { _ = CheckForUpdatesAsync(); };
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleThemeMenuItem_Click(object sender, RoutedEventArgs e) => ThemeService.Toggle();

    private void TutorialButton_Click(object sender, RoutedEventArgs e)
    {
        var tutorial = new TutorialWindow { Owner = this };
        tutorial.ShowDialog();
    }

    // Same deploy-time-config pattern as Minion.Agent/Minion.InjectionLab -- empty/missing
    // means "don't check," not an error, since running fully offline is a normal,
    // supported setup, not a broken one.
    private static string ReadUpdateManifestUrl()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            return doc.RootElement.TryGetProperty("UpdateManifestUrl", out var value) ? value.GetString() ?? "" : "";
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return "";
        }
    }

    // Enhancement-only: wrapped whole in try/catch, never surfaces an error, never
    // delays/blocks startup. Only ever shows the banner (UpdateBanner in
    // MainWindow.xaml) -- no modal dialog, so it never gets in the way of whatever
    // the user's doing.
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var manifestUrl = ReadUpdateManifestUrl();
            if (string.IsNullOrWhiteSpace(manifestUrl))
            {
                return;
            }

            var settings = _updateCheckService.LoadSettings();
            if (!UpdateCheckService.IsDue(settings, DateTime.UtcNow))
            {
                return;
            }

            var manifest = await _updateCheckService.CheckAsync(manifestUrl);
            _updateCheckService.SaveSettings(settings with { LastCheckedAtUtc = DateTime.UtcNow });

            if (manifest is null
                || manifest.LatestVersion == settings.SkippedVersion
                || !UpdateCheckService.IsNewerThanRunning(manifest.LatestVersion))
            {
                return;
            }

            _pendingUpdateManifest = manifest;
            var featureNote = manifest.Features is { Count: > 0 }
                ? $" New: {string.Join(", ", manifest.Features.Take(2).Select(f => f.Title))}{(manifest.Features.Count > 2 ? ", and more" : "")}."
                : "";
            UpdateBannerText.Text = $"Minion.CryptoLab {manifest.LatestVersion} is available.{featureNote}";
            UpdateBannerWhatsNewButton.Visibility = string.IsNullOrWhiteSpace(manifest.ReleaseNotesUrl) ? Visibility.Collapsed : Visibility.Visible;
            UpdateBannerDownloadButton.Visibility = string.IsNullOrWhiteSpace(manifest.DownloadUrl) ? Visibility.Collapsed : Visibility.Visible;
            UpdateBanner.Visibility = Visibility.Visible;
        }
        catch
        {
            // Never let a broken/unreachable manifest affect startup.
        }
    }

    private static void OpenInBrowser(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    private void UpdateBannerWhatsNew_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingUpdateManifest?.ReleaseNotesUrl is { Length: > 0 } url)
        {
            OpenInBrowser(url);
        }
    }

    private void UpdateBannerDownload_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingUpdateManifest?.DownloadUrl is { Length: > 0 } url)
        {
            OpenInBrowser(url);
        }
    }

    // "Dismiss" means "skip this version" -- it won't ask again until a newer one
    // is published. There's no separate "remind me later" button: the banner isn't
    // a nag dialog blocking anything, so simply leaving it up (or closing the app
    // and reopening before dismissing) already covers that case for free.
    private void UpdateBannerDismiss_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingUpdateManifest is not null)
        {
            var settings = _updateCheckService.LoadSettings();
            _updateCheckService.SaveSettings(settings with { SkippedVersion = _pendingUpdateManifest.LatestVersion });
        }

        UpdateBanner.Visibility = Visibility.Collapsed;
    }
}
