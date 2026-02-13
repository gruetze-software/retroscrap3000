using System.Reflection;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using Serilog;

namespace RetroScrap3000.Models;

public static class Tools
{
    public static string GetVersion()
    {
        // 1. Titelleiste aus Assembly-Infos
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetName().Version?.ToString(3) ?? "1.0.0";
    }

    public static string GetAppTitle()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var title = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "C64U Slim-Viewer";
        var version = GetVersion();
        var author = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Grütze-Software";
        return $"{title} v{version} by {author}";
    }

    public static string GetExcMsg(Exception ex)
    {
        string msg = ex.Message;
        if (ex.InnerException != null && ex.InnerException.Message != msg)
            msg += "\r\n" + GetExcMsg(ex.InnerException);
        return msg;
    }

    public static int CalculatePercentage(int current, int total)
    {
        if (total == 0)
        {
            return 0;
        }

        // Vermeide Ganzzahldivision durch die Multiplikation mit 100.0 (einem double)
        return (int)((current / (double)total) * 100);
    }

    public static string GetNameFromFile(string? filePath)
    {
        // Liefert den Dateinamen OHNE Erweiterung zurück.
        // Path.GetFileNameWithoutExtension() kann das auch, aber bei
        // Namen wie "Dr. Mario.zip" liefert es nur "Dr." zurück.
        // Daher hier eine eigene Implementierung.

        if (string.IsNullOrWhiteSpace(filePath))
            return string.Empty;
        
        string filename = Path.GetFileName(filePath);
        int index = filename.LastIndexOf('.');
        if (index < 0)
            return filename.Trim(); // keine Erweiterung
        if ( index == 0)
            return string.Empty; // nur Erweiterung, kein Name
        
        return filename.Substring(0, index).Trim(); // Name ohne Erweiterung
    }

    public static string NormalizeRelativePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // 1. Alle Backslashes (\) durch Forward Slashes (/) ersetzen (Standard in Batocera/Linux)
        string normalizedPath = path.Replace('\\', '/');

        // 2. Führendes "./" oder nur "." entfernen, falls vorhanden
        if (normalizedPath.StartsWith("./"))
        {
            normalizedPath = normalizedPath.Substring(2);
        }
        else if (normalizedPath.StartsWith("."))
        {
            // Falls nur ein Punkt ohne Slash vorkommt
            normalizedPath = normalizedPath.Substring(1);
        }

        return normalizedPath;
    }

    public static string? ResolveMediaPath(string? systemDir, string? filepath)
    {
        if (string.IsNullOrEmpty(systemDir))
            return null;

        if (string.IsNullOrWhiteSpace(filepath)) return null;

        // Bereits absolut?
        if (Path.IsPathRooted(filepath)) return filepath;

        // "./" entfernen und Slashes normalisieren
        var rel = filepath.Trim();
        if (rel.StartsWith("./") || rel.StartsWith(".\\"))
            rel = rel.Substring(2);

        rel = rel.Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);

        return Path.Combine(systemDir, rel);
    }

    /// <summary>
    /// Erstellt eine M3U-Datei aus einer Liste von GameEntry-Objekten (Discs).
    /// </summary>
    /// <param name="romDirectory">Das Basisverzeichnis der ROMs.</param>
    /// <param name="selectedGames">Die ausgewählten GameEntry-Objekte (z.B. Disk 1, Disk 2, etc.).</param>
    /// <returns>Der Name der erzeugten M3U-Datei, oder null bei Fehler.</returns>
    public static string? CreateM3uFromGames(string romDirectory, IEnumerable<GameEntry> selectedGames)
    {
        var gameList = selectedGames.ToList();
        if (gameList.Count < 2) return null; // Mindestens zwei Discs benötigt

        // 1. Konsistenzprüfung: Alle Discs müssen im gleichen Unterordner liegen
        string? commonBaseDir = Path.GetDirectoryName(Tools.ResolveMediaPath(romDirectory, gameList[0].Path));
        if (commonBaseDir == null) return null;

        // 2. Namensfindung für die M3U-Datei
        // Wir nehmen den Namens-Teil vor der Disc-Nummer und verwenden ihn als M3U-Namen.
        // Beispiel: "Game (Disk 1).chd" -> "Game.m3u"
        string baseName = GetBaseNameForM3u(Tools.GetNameFromFile(gameList[0].FileName!));
        if (string.IsNullOrEmpty(baseName)) baseName = "MultiDiscGame"; // Fallback

        // Erstelle den vollständigen Pfad zur M3U-Datei
        string m3uFileName = $"{baseName}.m3u";
        string m3uPath = Path.Combine(commonBaseDir, m3uFileName);

        var sb = new StringBuilder();

        // 3. Inhalt generieren (relativ zur M3U-Datei)
        foreach (var game in gameList.OrderBy(g => g.Path)) // Optional: Sortiere nach Pfad/Name, um Reihenfolge zu sichern
        {
            // Wir brauchen nur den Dateinamen relativ zum M3U-Verzeichnis
            string discFileName = game.FileName!;
            sb.AppendLine(discFileName);
        }

        // 4. M3U-Datei schreiben
        try
        {
            File.WriteAllText(m3uPath, sb.ToString(), Encoding.UTF8);

            // 5. Cleanup (optional, aber empfohlen): Originale Einträge markieren
            // Im UI-Kontextmenü müssen Sie anschließend:
            // a) die gamelist.xml mit den einzelnen Discs bereinigen (am besten mit Ihrer Clean-Funktion)
            // b) die einzelnen ROM-Dateien löschen/verschieben (falls gewünscht)
            // c) EINEN NEUEN GameEntry für die M3U-Datei erstellen und zur Roms.Games Liste hinzufügen.

            return m3uFileName;
        }
        catch (Exception ex)
        {
            // Fehlerbehandlung
            Log.Fatal(ex, $"Error create m3u-entry.");
            return null;
        }
    }

    private static string GetBaseNameForM3u(string gameName)
    {
        // Entferne typische Disc- oder Part-Bezeichnungen
        var cleanedName = Regex.Replace(gameName, @"\s+\(\s*(Disc|Disk|CD|Part)\s*\d+\s*\)", "", RegexOptions.IgnoreCase);
        cleanedName = Regex.Replace(cleanedName, @"\s+\[\s*(Disc|Disk|CD|Part)\s*\d+\s*\]", "", RegexOptions.IgnoreCase);
        return cleanedName.Trim();
    }

    /// <summary>
    /// Liest eine M3U-Datei und extrahiert alle referenzierten Dateipfade.
    /// </summary>
    /// <param name="m3uPath">Der absolute Pfad zur M3U-Datei.</param>
    /// <returns>Eine Liste von relativen Pfaden der Disc-Dateien (relativ zum M3U-Ordner).</returns>
    public static List<string> GetM3uReferencedFiles(string m3uPath)
    {
            var referencedFiles = new List<string>();

            if (!File.Exists(m3uPath))
                return referencedFiles;

            try
            {
                // M3U-Dateien sind einfache Textdateien.
                var lines = File.ReadAllLines(m3uPath);
                var m3uDirectory = Path.GetDirectoryName(m3uPath);

                if (m3uDirectory == null)
                    return referencedFiles;

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                    {
                        continue; // Überspringe leere Zeilen und Kommentare
                    }

                    // Normalerweise enthält die M3U nur den Dateinamen (relativ zum M3U-Ordner)
                    // Beispiel: Resident Evil (Disc 1).chd
                    // Da wir im Haupt-Load-Loop den Pfad relativ zum "romDirectory" benötigen, 
                    // müssen wir ihn entsprechend auflösen.

                    // 1. Absoluten Pfad der Disc-Datei ermitteln
                    string absoluteDiscPath = Path.Combine(m3uDirectory, trimmedLine);

                    // 2. Relativen Pfad zum Haupt-ROM-Ordner (romDirectory) ermitteln
                    // (Hier ist eine Annahme über Ihre FileTools.ResolveMediaPath Logik, ansonsten 
                    // müssen Sie den relativen Pfad direkt berechnen.)
                    string? relativeDiscPath = Path.GetRelativePath(m3uDirectory, absoluteDiscPath);
                    
                    // Stellen Sie sicher, dass es das gewünschte Format (z.B. './' Präfix oder nur der relative Pfad) hat.
                    // Für den Vergleich in 'm3uReferencedPaths' sollte es das NORMALISIERTE Format sein.
                    var normalizedPath = NormalizeRelativePath("./" + relativeDiscPath?.Replace('\\', '/'));

                    referencedFiles.Add(normalizedPath);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Error reading M3U-File \"{m3uPath}\".");
            }

            return referencedFiles;
    }

    public static string GetOSName()
    {
        string os = Environment.OSVersion.ToString();
        string arch = RuntimeInformation.OSArchitecture.ToString();
        return $"{os} {arch}";
    }

    public class Checksums
    {
            public string? File { get; set; }
            public string CRC32 { get; set; } = "";
            public string MD5 { get; set; } = "";
            public string SHA1 { get; set; } = "";

            public Checksums(string? file) { File = file; }
    }

}
