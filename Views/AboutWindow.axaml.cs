using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Triangle3DAnimationGUI.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnMokuuraClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Mokuura/Triangle3DAnimation",
                UseShellExecute = true
            });
        }
        catch { /* Ignorer les erreurs d'ouverture de lien */ }
    }

    private void OnTractorFanClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/SuperKulPerson",
                UseShellExecute = true
            });
        }
        catch { /* Ignorer les erreurs d'ouverture de lien */ }
    }

    private void OnSuperCobyClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/SuperCoby",
                UseShellExecute = true
            });
        }
        catch { /* Ignorer les erreurs d'ouverture de lien */ }
    }
}
