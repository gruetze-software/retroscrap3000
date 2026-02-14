using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RetroScrap3000.Helpers;
using Serilog;

namespace RetroScrap3000.Models;

public class AppSettings
{
    public string RomPath { get; set; } = string.Empty;
    public string LastUsedSystem { get; set; } = string.Empty;
    public bool DarkMode { get; set; } = true;
    public string? Language { get; set; }
    public string? Region { get; set; }
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

    public bool? MediaBoxImage { get; set; }
    public bool? MediaBox2DFront { get; set; }
    public bool? MediaBox3DFront { get; set; }
    public bool? MediaBoxSide { get; set; }
    public bool? MediaBoxBack { get; set; }
    public bool? MediaBoxTextures { get; set; }
    public bool? MediaBoxMix1 { get; set; }
    public bool? MediaBoxMix2 { get; set; }
    public bool? MediaVideo { get; set; }
    public bool? MediaMarquee { get; set; }
    public bool? MediaFanart { get; set; }
    public bool? MediaScreenshot { get; set; }
    public bool? MediaScreenshotTitle { get; set; }
    public bool? MediaWheel { get; set; }
    public bool? MediaWheelSteel { get; set; }
    public bool? MediaWheelCarbon { get; set; }
    public bool? MediaManual { get; set; }
    public bool? MediaMap { get; set; }

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
            Log.Information($"Load {FilePath}");
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings(); 
        }
        catch (Exception ex)
        { 
            Log.Fatal(ex, "Exception load settings.json");
            return new AppSettings(); 
        }
    }

    public void Save()
    {
        Log.Information($"Save {FilePath}");
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
    }

    public string GetLanguageShortCode()
    {
        if (string.IsNullOrEmpty(Language))
            return "en"; // default
        if ( Language.Contains('-') == false )
            return Language.ToLower(); // already short code
        
        var parts = Language.Split('-');
        return parts[0];
    }

    public static GameMediaSettings? GetMediaSettings(eMediaType mediaType)
    {
            return GetMediaSettingsList().FirstOrDefault(x => x.Type == mediaType);
    }

    public static List<GameMediaSettings> GetMediaSettingsList()
    {
        return new List<GameMediaSettings>
        {
            { new GameMediaSettings(eMediaType.ScreenshotTitle, "sstitle", "title" ) },
            { new GameMediaSettings(eMediaType.ScreenshotGame, "ss", "screenshot") },
            { new GameMediaSettings(eMediaType.Fanart, "fanart", "fanart") },
            { new GameMediaSettings(eMediaType.Video, "video", "video") },
            { new GameMediaSettings(eMediaType.Marquee, "screenmarquee", "marquee") },
            { new GameMediaSettings(eMediaType.Manual, "manuel", "manual") },
            { new GameMediaSettings(eMediaType.Map, "map", "map") },
            { new GameMediaFront(eMediaBoxFrontType.TwoDim) },
            { new GameMediaFront(eMediaBoxFrontType.ThreeDim) },
            { new GameMediaFront(eMediaBoxFrontType.Mix1) },
            { new GameMediaFront(eMediaBoxFrontType.Mix2) },
            { new GameMediaSettings(eMediaType.BoxImageSide, "box-2D-side", "side") },
            { new GameMediaSettings(eMediaType.BoxImageBack, "box-2D-back", "back") },
            { new GameMediaSettings(eMediaType.BoxImageTexture, "box-texture", "texture") },
            { new GameMediaWheel(eMediaWheelType.Normal) },
            { new GameMediaWheel(eMediaWheelType.Steel) },
            { new GameMediaWheel(eMediaWheelType.Carbon) },
        };
    }

    public bool IsMediaTypeEnabled(GameMediaSettings media)
    {
        if (media.Type == eMediaType.Wheel)
        {
            GameMediaWheel wheel = (GameMediaWheel)media;
            if ( wheel.WheelType == eMediaWheelType.Normal )
                return this.MediaWheel.HasValue && this.MediaWheel.Value == true ? true : false;
            else if (wheel.WheelType == eMediaWheelType.Steel)
                return this.MediaWheelSteel.HasValue && this.MediaWheelSteel.Value == true ? true : false;
            if (wheel.WheelType == eMediaWheelType.Carbon)
                return this.MediaWheelCarbon.HasValue && this.MediaWheelCarbon.Value == true ? true : false;
            else
                return false;
        }
        else if (media.Type == eMediaType.Fanart) return this.MediaFanart.HasValue && this.MediaFanart.Value == true ? true : false;
        else if (media.Type == eMediaType.Marquee) return this.MediaMarquee.HasValue && this.MediaMarquee.Value == true ? true : false;
        else if (media.Type == eMediaType.Map) return this.MediaMap.HasValue && this.MediaMap.Value == true ? true : false;
        else if (media.Type == eMediaType.Manual) return this.MediaManual.HasValue && this.MediaManual.Value == true ? true : false;
        else if (media.Type == eMediaType.Video) return this.MediaVideo.HasValue && this.MediaVideo.Value == true ? true : false;
        else if (media.Type == eMediaType.ScreenshotTitle) return this.MediaScreenshotTitle.HasValue && this.MediaScreenshotTitle.Value == true ? true : false;
        else if (media.Type == eMediaType.ScreenshotGame) return this.MediaScreenshot.HasValue && this.MediaScreenshot.Value == true ? true : false;
        else if (media.Type == eMediaType.BoxImageFront) 
        {
            if (!this.MediaBoxImage.HasValue || this.MediaBoxImage.Value == false)
                return false;

            GameMediaFront front = (GameMediaFront)media;
            if ( front.FrontType == eMediaBoxFrontType.TwoDim )
                return this.MediaBox2DFront.HasValue && this.MediaBox2DFront.Value == true ? true : false;
            else if (front.FrontType == eMediaBoxFrontType.ThreeDim )
                return this.MediaBox3DFront.HasValue && this.MediaBox3DFront.Value == true ? true : false;
            else if (front.FrontType == eMediaBoxFrontType.Mix1)
                return this.MediaBoxMix1.HasValue && this.MediaBoxMix1.Value == true ? true : false;
            else if (front.FrontType == eMediaBoxFrontType.Mix2)
                return this.MediaBoxMix2.HasValue && this.MediaBoxMix2.Value == true ? true : false;
            else return false;
        }
        else if (media.Type == eMediaType.BoxImageBack) return this.MediaBoxBack.HasValue && this.MediaBoxBack.Value == true ? true : false;
        else if (media.Type == eMediaType.BoxImageSide) return this.MediaBoxSide.HasValue && this.MediaBoxSide.Value == true ? true : false;
        else if (media.Type == eMediaType.BoxImageTexture) return this.MediaBoxTextures.HasValue && this.MediaBoxTextures.Value == true ? true : false;
        else return false;
    }
}