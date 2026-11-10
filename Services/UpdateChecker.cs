using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Overtime;

public record AvailableUpdate(Version Version, string Tag, string ReleaseNotes);

// Looks for a newer GitHub release and installs it with install.ps1.
public static class UpdateChecker
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/ollebaack/Overtime/releases/latest";
    private const string InstallScriptUrl = "https://raw.githubusercontent.com/ollebaack/Overtime/main/install.ps1";
    private const string SkippedVersionKey = "SkippedUpdateVersion";

#if DEBUG
    private static readonly bool IsDebugBuild = true;
#else
    private static readonly bool IsDebugBuild = false;
#endif

    private record GitHubRelease(string TagName, string? Body);

    // Release builds get their version from the git tag, e.g. v1.3.0 -> 1.3.0.0.
    public static Version CurrentVersion { get; } = Normalize(typeof(UpdateChecker).Assembly.GetName().Version ?? new Version(0, 0));

    // Returns null when up to date, skipped, offline, or not a tagged release build.
    public static async Task<AvailableUpdate?> CheckAsync()
    {
        // Debug builds don't check; untagged CI builds are 0.0.<run number>.
        if (IsDebugBuild || !OperatingSystem.IsWindows() || (CurrentVersion.Major == 0 && CurrentVersion.Minor == 0))
        {
            return null;
        }

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd($"Overtime/{CurrentVersion}");

            var release = await http.GetFromJsonAsync<GitHubRelease>(
                LatestReleaseUrl, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            if (release is null || !Version.TryParse(release.TagName.TrimStart('v'), out var latest))
            {
                return null;
            }

            latest = Normalize(latest);
            if (latest <= CurrentVersion || Preferences.Get(SkippedVersionKey, "") == latest.ToString())
            {
                return null;
            }

            return new AvailableUpdate(latest, release.TagName, release.Body ?? "");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update check failed: {ex.Message}");
            return null;
        }
    }

    public static void Skip(AvailableUpdate update) => Preferences.Set(SkippedVersionKey, update.Version.ToString());

    // Runs the installer in a visible PowerShell window and starts the new version when it's done.
    // The installer stops this app before replacing its files.
    public static void StartInstall()
    {
        var script = $$"""
            try {
                irm '{{InstallScriptUrl}}' | iex
                Start-Process (Join-Path $env:LOCALAPPDATA 'Programs\Overtime\Overtime.exe')
            } catch {
                Write-Host "Update failed: $_" -ForegroundColor Red
                Read-Host 'Press Enter to close'
            }
            """;

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script)),
            UseShellExecute = true,
        });
    }

    private static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0));
}
