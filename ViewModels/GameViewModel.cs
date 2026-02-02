using ReactiveUI;
using Avalonia.Media;
using RetroScrap3000.Models;

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

    public string? Developer => Entry.Developer;
    public string? Genre => Entry.Genre;
    public string? Description => Entry.Description;
    public double RatingStars => Entry.RatingStars;

    // Status-Farbe für die Liste
    public IBrush StatusColor => Entry.State switch
    {
        eState.Scraped => Brushes.LightGreen,
        eState.Error => Brushes.Salmon,
        _ => Brushes.Gray
    };
}