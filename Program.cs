using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Serilog;
using System.IO;
using RetroScrap3000.Models;

namespace RetroScrap3000;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // 1. Log-Pfad bestimmen (Plattformübergreifend)
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logDir = Path.Combine(appData, "RetroScrap3000", "logs");
        Directory.CreateDirectory(logDir);
        var logFile = Path.Combine(logDir, "log.txt");

        // 2. Serilog initialisieren
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("App startet...");
            Log.Information($"{Tools.GetOSName()} erkannt.");
            // 3. Avalonia App starten
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            
            
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "App ist unerwartet abgestürzt!");
        }
        finally
        {
            // Sicherstellen, dass alle Logs geschrieben werden
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
