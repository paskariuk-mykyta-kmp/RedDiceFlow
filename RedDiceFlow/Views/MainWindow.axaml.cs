using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RedDiceFlow.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();


        MainContentArea.Content = new DashboardView();
        BtnDash.Background = Avalonia.Media.Brush.Parse("#C3073F");
    }

    private void OnMenuClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            
            BtnDash.Background = Avalonia.Media.Brush.Parse("#252525");
            BtnGames.Background = Avalonia.Media.Brush.Parse("#252525");
            BtnAnalytic.Background = Avalonia.Media.Brush.Parse("#252525");
            BtnSettings.Background = Avalonia.Media.Brush.Parse("#252525");

            
            button.Background = Avalonia.Media.Brush.Parse("#C3073F");

            
            switch (button.Name)
            {
                case "BtnDash": MainContentArea.Content = new DashboardView(); break;
                case "BtnGames": MainContentArea.Content = new GamesView(); break;
                case "BtnAnalytic": MainContentArea.Content = new AnalyticsView(); break;
                case "BtnSettings": MainContentArea.Content = new SettingsView(); break;
            }
        }
    }
}