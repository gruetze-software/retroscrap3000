using ReactiveUI;
using RetroScrap3000.Models;
using Avalonia; // Wichtig für ThemeVariant
using Avalonia.Styling;
using System.Reactive;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using RetroScrap3000.Services;

namespace RetroScrap3000.ViewModels;

public class OptionsViewModel : ViewModelBase
{
    public class ApiResultLine
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
    public ObservableCollection<ApiResultLine> ApiDetails { get; } = new();

    private bool _isTesting;
    public bool IsTesting
    {
        get => _isTesting;
        set => this.RaiseAndSetIfChanged(ref _isTesting, value);
    }


    private readonly AppSettings _settings;
    private readonly bool _initialDarkMode; // Zum Speichern des alten Zustands

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
            IsTesting = true;
            ApiDetails.Clear();

            var apiresult = await Task.Run(() => TryLogin()); 
            foreach (var line in apiresult)
            {
                ApiDetails.Add(line);
            }

            IsTesting = false;
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

    private async Task<List<ApiResultLine>> TryLogin()
    {
        List<ApiResultLine> result = new List<ApiResultLine>();
        if (string.IsNullOrEmpty(ScrapUser) ) 
            result.Add(new ApiResultLine { Label = "Error:", Value = "Username is empty" });
        if (string.IsNullOrEmpty(ScrapPwd) )
            result.Add(new ApiResultLine { Label = "Error:", Value = "Password is empty" });
        if (result.Count > 0)
            return result;
        
        try
        {
            ScraperManager manager = new ScraperManager();
            manager.RefreshSecrets(ScrapUser, ScrapPwd);
            var testdata = await manager.FetchSsUserInfosAsync();
            if (testdata == null || testdata.response == null || testdata.response.ssuser == null)
            {
               result.Add(new ApiResultLine { Label = "Error:", Value = "No user data received" });
               return result;
            }
            else
            {
                var user = testdata.response.ssuser;
                result.Add(new ApiResultLine { Label = "Username:", Value = user.id ?? "-" });
                result.Add(new ApiResultLine { Label = "Level:", Value = user.niveau != null ? user.niveau.Value.ToString() : "-" });
                result.Add(new ApiResultLine { Label = "Last Login:", Value = user.LastVisit() != null ? user.LastVisit()!.Value.ToString() : "-" });
                result.Add(new ApiResultLine { Label = "Requests Today:", Value = user.requeststoday != null ? user.requeststoday.Value.ToString() : "-" });
                result.Add(new ApiResultLine { Label = "Requests per Day:", Value = user.GetQuotaToday() });
                result.Add(new ApiResultLine { Label = "Max Requests per Day:", Value = user.maxrequestsperday != null ? user.maxrequestsperday.Value.ToString() : "-" });
                result.Add(new ApiResultLine { Label = "Used Today (%):", Value = user.UsedTodayPercent().ToString("F2") });
                result.Add(new ApiResultLine { Label = "Max Threads:", Value = user.maxthreads != null ? user.maxthreads.Value.ToString() : "-" });
                result.Add(new ApiResultLine { Label = "Favorite Region:", Value = user.favregion ?? "-" });
                result.Add(new ApiResultLine { Label = "Visits:", Value = user.visites != null ? user.visites.Value.ToString() : "-" });
            }    
        }
        catch (Exception ex)
        {
            result.Add(new ApiResultLine { Label = "Error:", Value = Tools.GetExcMsg(ex) });
        }

        return result;
    }

    public void SaveAndClose()
    {
        Trace.WriteLine("SaveAndClose()");
        _settings.Save();
    }
}