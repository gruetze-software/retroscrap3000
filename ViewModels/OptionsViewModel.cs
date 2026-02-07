using ReactiveUI;
using RetroScrap3000.Models;
using Avalonia; // Wichtig für ThemeVariant
using Avalonia.Styling;
using System.Reactive;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace RetroScrap3000.ViewModels;

public class OptionsViewModel : ViewModelBase
{
    private readonly AppSettings _settings;
    private readonly bool _initialDarkMode; // Zum Speichern des alten Zustands

    private string _testStatus = string.Empty;
    public string TestStatus
    {
        get => _testStatus;
        set => this.RaiseAndSetIfChanged(ref _testStatus, value);
    }

    public ReactiveCommand<Unit, Unit> TestConnectionCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public OptionsViewModel(AppSettings settings)
    {
        _settings = settings;
        _initialDarkMode = settings.DarkMode; // Stand beim Öffnen merken

        // OK-Logik: Erst speichern, dann "Erfolg" zurückgeben
        SaveCommand = ReactiveCommand.Create(() => {
            SaveAndClose();
        });

        // Abbrechen-Logik
        CancelCommand = ReactiveCommand.Create(() => {
            // Nichts speichern
            // ROLLBACK: Wenn abgebrochen wird, setzen wir den alten Modus wieder
            DarkMode = _initialDarkMode;
        });

        TestConnectionCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            TestStatus = "Prüfe Verbindung...";
            
            // Hier kommt deine tatsächliche Scraping-API Logik rein
            bool success = await Task.Run(() => TryLogin()); 

            if (success)
                TestStatus = "✅ Login erfolgreich!";
            else
                TestStatus = "❌ Login fehlgeschlagen. Bitte Daten prüfen.";
        });
    }

    public bool DarkMode
    {
        get => _settings.DarkMode;
        set 
        {
            if (_settings.DarkMode != value)
            {
                _settings.DarkMode = value;
                this.RaisePropertyChanged();

                // LIVE THEME SWITCH
                if (Application.Current != null)
                {
                    Application.Current.RequestedThemeVariant = value 
                        ? ThemeVariant.Dark 
                        : ThemeVariant.Light;
                }
            }
        }
    }

    public bool ScanOnStart
    {
        get => _settings.ScanOnStart;
        set 
        {
            if (_settings.ScanOnStart != value)
            {
                _settings.ScanOnStart = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public bool Logging
    {
        get => _settings.Logging;
        set 
        {
            if (_settings.Logging != value)
            {
                _settings.Logging = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public string ScrapUser
    {
        get => _settings.ScrapUser;
        set 
        {
            if (_settings.ScrapUser != value)
            {
                _settings.ScrapUser = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public string ScrapPwd
    {
        get => _settings.ClearScrapPwd; // Greift auf die Decrypt-Logik zu
        set 
        {
            if (_settings.ClearScrapPwd != value)
            {
                _settings.ClearScrapPwd = value; // Löst die Encrypt-Logik aus
                this.RaisePropertyChanged();
            }
        }
    }

    private async Task<bool> TryLogin()
    {
        if (string.IsNullOrEmpty(ScrapUser) || string.IsNullOrEmpty(ScrapPwd)) 
            return false;
        
        try
        {
            ScraperManager manager = new ScraperManager();
            manager.RefreshSecrets(ScrapUser, ScrapPwd);
            var testdata = await manager.FetchSsUserInfosAsync();
            if (testdata == null || testdata.response == null || testdata.response.ssuser == null)
            {
                /*
                listBoxApiTest.Items.Clear();
                listBoxApiTest.Items.AddRange(
                    [ Properties.Resources.Txt_Api_Err_NoResponse,
                        Properties.Resources.Txt_Api_Err_CheckInternet ]);
                        */
                return false;
            }
            else
            {
                    //listBoxApiTest.Items.Clear();
                    var user = testdata.response.ssuser;
                    /*
                    listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Name, user.id ?? ""));
                    listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Level, user.niveau != null ? user.niveau.Value : "-"));
                    listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_LastLogin, user.LastVisit() != null ? user.LastVisit()!.Value.ToString() : "-"));
                    listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Visits, user.visites != null ? user.visites.Value : "-"));
                    listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Region, user.favregion ?? ""));
                    listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_MaxThreads, user.maxthreads != null ? user.maxthreads : "-"));
                    listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_DownloadKBS, user.MaxDownloadSpeed != null ? user.MaxDownloadSpeed : "-"));
                    listBoxApiTest.Items.Add(user.GetQuotaToday());
                    */
                    return true;
            }
        }
        catch (Exception ex)
        {
            /*
                listBoxApiTest.Items.Clear();
                listBoxApiTest.Items.Add("Fail!");
                listBoxApiTest.Items.Add(ex.Message);
                Log.Error($"{Utils.GetExcMsg(ex)}");
                */
                Trace.WriteLine($"Exception {Tools.GetExcMsg(ex)}");
            return false;
        }
    }

    public void SaveAndClose()
    {
        Trace.WriteLine("SaveAndClose()");
        _settings.Save();
    }
}