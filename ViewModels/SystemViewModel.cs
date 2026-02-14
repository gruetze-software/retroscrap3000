using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using ReactiveUI;
using RetroScrap3000.Models;

namespace RetroScrap3000.ViewModels;

public class SystemViewModel : ViewModelBase
{
    public string SystemName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public Bitmap? SystemIcon { get; private set; }
    public Bitmap? SystemBanner { get; private set; }

    // Das Herzstück: Jedes System trägt seine ROMs selbst
    public ObservableCollection<GameEntry> Roms { get; } = new();

    // Referenz auf die Logik-Daten (falls benötigt)
    public RetroSystem System { get; set; } = new();

    public SystemViewModel(RetroSystem sys)
    {
        System = sys;
        SystemName = System.Name_eu ?? "Unknown";
        Manufacturer = System.Hersteller ?? "Unkown";
        Year = System.Debut.ToString();
        LoadImages();
    }

    private void LoadImages()
    {
        // Icon laden
        if (!string.IsNullOrEmpty(System.FileIcon) && File.Exists(System.FileIcon))
        {
            try { SystemIcon = new Bitmap(System.FileIcon); } catch { /* Log error */ }
        }

        // Banner laden
        if (!string.IsNullOrEmpty(System.FileBanner) && File.Exists(System.FileBanner))
        {
            try { SystemBanner = new Bitmap(System.FileBanner); } catch { /* Log error */ }
        }
    }
}