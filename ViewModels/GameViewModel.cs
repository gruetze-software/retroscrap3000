using ReactiveUI;
using Avalonia.Media;
using RetroScrap3000.Models;
using System;
using System.IO;

namespace RetroScrap3000.ViewModels;

public class GameViewModel : ViewModelBase
{
    // Das Original-Model von GitHub
    public GameEntry Entry { get; }

    public GameViewModel(GameEntry entry)
    {
        Entry = entry;
    }

    // Properties, die die UI "beobachtet"
    public string Name
    {
        get => Entry.Name ?? Entry.FileName ?? "Unknown";
        set
        {
            if (Entry.Name != value)
            {
                Entry.Name = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public bool Favorite
    {
        get => Entry.Favorite;
        set
        {
            if (Entry.Favorite != value)
            {
                Entry.Favorite = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public string? Developer
    {
        get => Entry.Developer;
        set
        {
            if (Entry.Developer != value)
            {
                Entry.Developer = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public string? Genre
    {
        get => Entry.Genre;
        set
        {
            if (Entry.Genre != value)
            {
                Entry.Genre = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public string? Description
    {
        get => Entry.Description;
        set
        {
            if (Entry.Description != value)
            {
                Entry.Description = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public string? Players
    {
        get => Entry.Players;
        set
        {
            if (Entry.Players != value)
            {
                Entry.Players = value;
                this.RaisePropertyChanged();
            }
        }
    }
    
    public string RatingStars
    {
        get
        {
            // Beispiel: 0.8 im Model entspricht 4 von 5 Sternen
            double val = Entry.Rating;
            int stars = (int)Math.Round(val * 5); 
            
            // Erzeugt einen String wie "★★★★☆"
            return new string('★', stars).PadRight(5, '☆');
        }
    }

    // Für die Bearbeitung lassen wir den numerischen Wert zusätzlich da
    public string RatingValue
    {
        get => (Entry.Rating * 10).ToString("F1") ?? "0.0"; // Anzeige 0-10 für leichtere Eingabe
        set
        {
            if (double.TryParse(value, out double result))
            {
                var newVal = result / 10.0;
                Entry.Rating = newVal;
                this.RaisePropertyChanged(nameof(RatingValue));
                this.RaisePropertyChanged(nameof(RatingStars)); // Sterne aktualisieren!
            }
        }
    }

    public DateTime? ReleaseDate
    {
        get => Entry.ReleaseDate;
        set
        {
            if (!Equals(Entry.ReleaseDate, value))
            {
                Entry.ReleaseDate = value;
                this.RaisePropertyChanged();
            }
        }
    } 

    public string FileNameShort 
    {
        get 
        {
            if (string.IsNullOrEmpty(Entry.Path)) return "-";
            return Path.GetFileName(Entry.Path);
        }
    }

    public string ReleaseYear => Entry.ReleaseDate?.Year.ToString() ?? "-";

    public string? Publisher
    {
        get => Entry.Publisher;
        set
        {
            if (Entry.Publisher != value)
            {
                Entry.Publisher = value;
                this.RaisePropertyChanged();
            }
        }
    }

    // Status-Farbe für die Liste
    public IBrush StatusColor => Entry.State switch
    {
        eState.Scraped => Brushes.LightGreen,
        eState.Error => Brushes.Salmon,
        _ => Brushes.Gray
    };
}