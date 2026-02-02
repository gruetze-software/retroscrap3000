using System.Reflection;
using System;
using System.IO;

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

}
