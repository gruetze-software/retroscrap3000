using System.Collections.ObjectModel;
using ReactiveUI;
using RetroScrap3000.Models;

namespace RetroScrap3000.ViewModels;

public class SystemViewModel : ViewModelBase
{
    private string _systemName = string.Empty;
    private string _folderPath = string.Empty;
    private RetroSystem? _system = null;

    public string SystemName
    {
        get => _systemName;
        set => this.RaiseAndSetIfChanged(ref _systemName, value);
    }

    public string FolderPath
    {
        get => _folderPath;
        set => this.RaiseAndSetIfChanged(ref _folderPath, value);
    }

    // Hier landen die GameViewModels, die wir vorhin erstellt haben
    public ObservableCollection<GameViewModel> Roms { get; } = new();

    public SystemViewModel(RetroSystem sys)
    {
        _system = sys;
        SystemName = _system.Name_eu;
        FolderPath = _system.RomFolderName;
    }
}