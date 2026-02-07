using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroScrap3000.ViewModels;

namespace RetroScrap3000.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public async void OpenOptions_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && vm.Settings != null)
        {
            var dialog = new OptionsWindow();
            dialog.DataContext = new OptionsViewModel(vm.Settings);
            await dialog.ShowDialog(this);
        }
    }
}