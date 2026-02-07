using System;
using System.Reactive; // Ermöglicht die Arbeit mit 'Unit'
using System.Reactive.Linq; // Stellt die nötigen Erweiterungsmethoden bereit
using ReactiveUI;
using RetroScrap3000.ViewModels;

namespace RetroScrap3000.Views;

public partial class OptionsWindow : Avalonia.ReactiveUI.ReactiveWindow<OptionsViewModel>
{
    public OptionsWindow()
    {
        InitializeComponent();

        // Diese Logik verknüpft das ViewModel-Signal mit der View-Aktion (Schließen)
        this.WhenActivated(d =>
        {
            // Wir sagen dem Compiler explizit, dass 'u' vom Typ Unit ist
            d(ViewModel!.SaveCommand.Subscribe((Unit u) => 
            {
                Close(true);
            }));

            d(ViewModel!.CancelCommand.Subscribe((Unit u) => 
            {
                Close(false);
            }));
        });
    }
}
