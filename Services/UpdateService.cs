
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Serilog;

namespace RetroScrap3000.Services
{
    public class UpdateService(string currentVersion)
    {
        private const string VersionUrl = "https://api.github.com/repos/gruetze-software/C64UViewer/releases/latest";
        private readonly string _currentVersion = currentVersion; // Deine aktuelle Version

        public async Task<(bool UpdateAvailable, string DownloadUrl, string VersionTag)> CheckForUpdates()
        {
            Log.Information("Checking for updates...");

            // Test
            //return (true, "https://www.google.com", "v9.9.9");

            using var client = new HttpClient();
            // GitHub verlangt zwingend einen User-Agent Header
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroScrap3000-Updater");

            try
            {
                var release = await client.GetFromJsonAsync<GitHubRelease>(VersionUrl);
                if (release != null 
                    && Version.TryParse(release.TagName.TrimStart('v', ' '), out var latestVersion) 
                    && Version.TryParse(_currentVersion, out var currentVersion))
                {
                    if (latestVersion > currentVersion)
                    {
                        return (true, release.HtmlUrl, release.TagName);
                    }
                }
            }
            catch (Exception Ex)
            {
                // Fehlerbehandlung (z.B. Logging) kann hier hinzugefügt werden
                Log.Fatal(Ex, $"Fehler bei der Update-Prüfung");
            }
            
            return (false, string.Empty, string.Empty);
        }
    }    
    
    public record GitHubRelease(
            [property: JsonPropertyName("tag_name")] string TagName,
            [property: JsonPropertyName("html_url")] string HtmlUrl
        );

};