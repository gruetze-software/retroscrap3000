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
using RetroScrap3000.Views;
using Avalonia.Threading;
using System.Linq;
using Serilog;

namespace RetroScrap3000.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region AppTitle and Updater

    private GameManager _gameManager;
    private ScraperManager _scraper;
    private readonly RetroSystems _systemsContainer = new();

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set { this.RaiseAndSetIfChanged(ref _statusText, value); Log.Information(_statusText); }
    }

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

    public AppSettings Settings { get; set; }

    public string RomPath
    {
        get => Settings.RomPath;
        set
        {
            if (Settings.RomPath != value)
            {
                Settings.RomPath = value;
                this.RaisePropertyChanged(); // Signal an UI
                Settings.Save();           // Automatisch speichern
            }
        }
    }
    
    private bool _isScanning;
    public bool IsScanning
    {
        get => _isScanning;
        set => this.RaiseAndSetIfChanged(ref _isScanning, value);
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

    public MainWindowViewModel(AppSettings settings)
    {
        _gameManager = new GameManager();
        _scraper = new ScraperManager();
        AppTitle = Tools.GetAppTitle();
        _version = Tools.GetVersion();
        Settings = settings;
       
        // Commands initialisieren...
        SelectFolderCommand = ReactiveCommand.CreateFromTask(async () => {
            await OpenFolderDialogAsync();
        });

        ScanCommand = ReactiveCommand.CreateFromTask(ExecuteScanAsync);

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

    public async Task InitializeAsync()
    {
        // 1. Zuerst lokal aus der JSON-Datei laden
        await Task.Run(() => _systemsContainer.Load());

        if (_systemsContainer.SystemList.Count == 0 || _systemsContainer.IsTooOld)
        {
            StatusText = "Systemliste veraltet oder fehlt. Lade von API...";
            await _systemsContainer.SetSystemsFromApiAsync(_scraper);
            _systemsContainer.Save();
        }
        
        StatusText = $"{_systemsContainer.SystemList.Count} Systeme geladen.";
    }

     private async Task ExecuteScanAsync()
    {
        if (IsScanning) return;
        
        IsScanning = true;
        StatusText = "Scanne ROMs...";

        try
        {
            // Wir führen den schweren Load-Vorgang im Hintergrund aus
            await Task.Run(() => 
            {
                // Deine vorhandene Methode im GameManager
                // Übergibt den Pfad und die Systemliste
                _gameManager.Load(Settings.RomPath, _systemsContainer);
            });

            // Nach dem Laden: UI aktualisieren (Systeme in die Liste werfen)
            await UpdateSystemListAfterScan();
        }
        finally
        {
            IsScanning = false;
            StatusText = "Scan abgeschlossen.";
        }
    }

    private async Task UpdateSystemListAfterScan()
    {
        // Auf dem UI-Thread arbeiten
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Systems.Clear();
            foreach (var sysModel in _gameManager.SystemList)
            {
                // Wir nutzen den GameManager, um zu sehen, ob für dieses System ROMs gefunden wurden
                // (Pass das an deine GameManager-Struktur an)
                var gamesFound = sysModel.Value;
                
                if (gamesFound.Games.Any())
                {
                    var displayItem = new SystemViewModel(gamesFound.RetroSys);
                    foreach (var game in gamesFound.Games)
                    {
                        displayItem.Roms.Add(game);
                    }
                    Systems.Add(displayItem);
                }
            }
        });
    }
}
