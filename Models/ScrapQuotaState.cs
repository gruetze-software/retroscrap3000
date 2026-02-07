using System;
using System.IO;
using System.Text.Json;

namespace RetroScrap3000.Models;

public class ScraperQuotaState
{
	public int LastReportedRequestsToday { get; set; } = 0;
	public int MaxRequestsPerDay { get; set; } = 0;
	public DateTime? LastUsageDate { get; set; } = null;

	private static string GetQuotaFile()
	{
        // Plattformübergreifend: ~/.config/RetroScrap3000/ unter Linux/Mac
        // AppData/Roaming unter Windows
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "RetroScrap3000", "Config");
        
        if (!Directory.Exists(configDir)) 
            Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, "scraperQuota.json");
	}

	public void Save()
	{
		var options = new JsonSerializerOptions
		{
			WriteIndented = true // hübsch formatiert
		};
		var json = JsonSerializer.Serialize(this, options);
		File.WriteAllText(GetQuotaFile(), json);
	}

	/// <summary>
	/// Lädt die Retrosysteme aus einer Json-Datei. 
	/// Falls die Datei nicht existiert, wird ein neues Objekt zurückgegeben.
	/// </summary>
	public static ScraperQuotaState? Load()
	{
		var file = GetQuotaFile();
		if (!File.Exists(file))
		{
			return null;
		}

		var json = File.ReadAllText(GetQuotaFile());
		return JsonSerializer.Deserialize<ScraperQuotaState>(json);
	}

}