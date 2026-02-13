using System.Collections.ObjectModel;
using RetroScrap3000.Models;

namespace RetroScrap3000.ViewModels;

public class RetroSystemDisplayItem : ViewModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;

    // Das Herzstück: Jedes System trägt seine ROMs selbst
    public ObservableCollection<GameEntry> Games { get; } = new();

    // Referenz auf die Logik-Daten (falls benötigt)
    public object? RawData { get; set; }
}