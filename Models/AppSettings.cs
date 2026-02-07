using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RetroScrap3000.Helpers;

namespace RetroScrap3000.Models;

public class AppSettings
{
    public string RomPath { get; set; } = string.Empty;
    public string LastUsedSystem { get; set; } = string.Empty;
    public bool DarkMode { get; set; } = true;
    public bool ScanOnStart { get; set; } = true;
    public bool Logging { get; set; } = false;
    public string ScrapUser { get; set; } = string.Empty;
    private string _scrapPwd = string.Empty;

    public string ScrapPwd
    {
        // Wir speichern den verschlüsselten String in der Datei
        get => _scrapPwd; 
        set => _scrapPwd = value;
    }

    // Hilfs-Property für das ViewModel (Klartext für die UI, verschlüsselt für die Datei)
    [JsonIgnore] // Damit diese Property NICHT im JSON landet
    public string ClearScrapPwd
    {
        get => CryptoHelper.Decrypt(_scrapPwd);
        set => _scrapPwd = CryptoHelper.Encrypt(value);
    }

    public static string FilePath
    {
        get 
        {
            // Erstellt den Pfad: /home/user/.config/c64uviewer/ (Linux) 
            // oder AppData/Roaming/c64uviewer/ (Windows)
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appData, "retroscrap3000");
                    
            // Ganz wichtig: Sicherstellen, dass der Ordner existiert!
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
                    
            return Path.Combine(configDir, "settings.json");
        }
    }

    public static AppSettings Load()
    {
        if (!File.Exists(FilePath)) 
            return new AppSettings();
        try 
        { 
            Trace.WriteLine($"Load {FilePath}");
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings(); 
        }
        catch (Exception ex)
        { 
            Trace.WriteLine("Exception load settings.json: " + Tools.GetExcMsg(ex));
            return new AppSettings(); 
        }
    }

    public void Save()
    {
        Trace.WriteLine($"Save {FilePath}");
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
    }
}