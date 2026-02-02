using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RetroScrap3000.Models;
using RetroScrap3000.Services;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace RetroScrap3000.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region AppTitle and Updater

    public string AppTitle { get; }

    private bool _isUpdateAvailable = false;
    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set => this.RaiseAndSetIfChanged(ref _isUpdateAvailable, value);
    }

    private string _version = string.Empty;
    public string Version
    {
        get => _version;
        set => this.RaiseAndSetIfChanged(ref _version, value);
    }
    private string _latestVersionText = string.Empty;
    public string LatestVersionText
    {
        get => _latestVersionText;
        set => this.RaiseAndSetIfChanged(ref _latestVersionText, value);
    }
    
    private string _downloadUrl = string.Empty;
    public string DownloadUrl
    {
        get => _downloadUrl;
        set => this.RaiseAndSetIfChanged(ref _downloadUrl, value);
    }
    #endregion

    private AppSettings _settings;

    public string RomPath
    {
        get => _settings.RomPath;
        set
        {
            if (_settings.RomPath != value)
            {
                _settings.RomPath = value;
                this.RaisePropertyChanged(); // Signal an UI
                _settings.Save();           // Automatisch speichern
            }
        }
    }
    
    // Commands für die Buttons
    public ReactiveCommand<Unit, Unit> SelectFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> ScanCommand { get; }
    public ReactiveCommand<Unit, Unit> OptionsCommand { get; }

    // 1. Die Liste der Systeme
    public ObservableCollection<SystemViewModel> Systems { get; } = new();

    // 2. Das aktuell gewählte System
    private SystemViewModel? _selectedSystem;
    public SystemViewModel? SelectedSystem
    {
        get => _selectedSystem;
        set => this.RaiseAndSetIfChanged(ref _selectedSystem, value);
    }

    // 3. Das aktuell gewählte Spiel
    private GameViewModel? _selectedGame;
    public GameViewModel? SelectedGame
    {
        get => _selectedGame;
        set => this.RaiseAndSetIfChanged(ref _selectedGame, value);
    }

    public MainWindowViewModel()
    {
        AppTitle = Tools.GetAppTitle();
        _version = Tools.GetVersion();
        _settings = AppSettings.Load(); 

        #region Test Data
        // Test:
        // 1. Ein System erstellen
        var snes = new SystemViewModel("Super Nintendo", "/home/user/roms/snes");
        
        // 2. Ein Test-Spiel hinzufügen (Wrapper um dein originales GameEntry)
        var game1 = new GameViewModel(new GameEntry 
        { 
            Name = "Super Mario World", 
            Developer = "Nintendo",
            Genre = "Platformer",
            Description = "Das klassische Abenteuer von Mario."
        });
        
        snes.Roms.Add(game1);
        
        // 3. Zur Liste der Systeme hinzufügen
        Systems.Add(snes);
        Systems.Add(new SystemViewModel("C64", "/home/user/roms/c64"));

        // Optional: Erstes System vor-selektieren
        SelectedSystem = snes;
        #endregion

        // Commands initialisieren...
        SelectFolderCommand = ReactiveCommand.CreateFromTask(async () => {
            await OpenFolderDialogAsync();
        });

        ScanCommand = ReactiveCommand.CreateFromTask(async () => {
            await OpenFolderDialogAsync();
        });

        OptionsCommand = ReactiveCommand.CreateFromTask(async () => {
            await OpenFolderDialogAsync();
        });
    }

    public async Task CheckVersionAsync()
    {
        var service = new UpdateService(_version);
        var (available, url, version) = await service.CheckForUpdates();
        
        if (available)
        {
            _downloadUrl = url;
            LatestVersionText = $"Update to {version} available (click to download)";
            IsUpdateAvailable = true;
        }
    }

    public void OpenUpdateUrl()
    {
        if (!string.IsNullOrEmpty(_downloadUrl))
        {
            Process.Start(new ProcessStartInfo(_downloadUrl) { UseShellExecute = true });
        }
    }

    private async Task OpenFolderDialogAsync()
    {
        // Zugriff auf das Hauptfenster erhalten
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel != null)
            {
                var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Wähle deinen ROM-Ordner",
                    AllowMultiple = false
                });

                if (result.Count > 0)
                {
                    // Den Pfad aus dem URI extrahieren
                    RomPath = result[0].Path.LocalPath;
                }
            }
        }
    }
}
