using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RedDiceFlow.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        
        MainContentArea.Content = new DashboardView();
    }

    private void OnMenuClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            switch (button.Name)
            {
                case "BtnDash":
                    MainContentArea.Content = new DashboardView();
                    break;
                case "BtnGames":
                    MainContentArea.Content = new GamesView();
                    break;
                case "BtnAnalytic":
                    MainContentArea.Content = new AnalyticsView();
                    break;
                case "BtnSettings":
                    MainContentArea.Content = new SettingsView();
                    break;
            }

            
            StatusDisplay.Text = "Розділ: " + button.Content;
        }
    }
}