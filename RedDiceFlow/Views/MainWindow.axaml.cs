using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RedDiceFlow.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContentArea.Content = new DashboardView();
            SetActiveButton(BtnDash);
        }

        private void OnMenuClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            SetActiveButton(button);

            switch (button.Name)
            {
                case "BtnDash":
                    MainContentArea.Content = new DashboardView();
                    break;
                case "BtnGames":
                    MainContentArea.Content = new GamesView();
                    break;
                case "BtnSales":
                    MainContentArea.Content = new SalesView();
                    break;
                case "BtnOrders":
                    MainContentArea.Content = new OrdersView();
                    break;
                case "BtnAnalytic":
                    MainContentArea.Content = new AnalyticsView();
                    break;
                case "BtnSettings":
                    MainContentArea.Content = new SettingsView();
                    break;
            }
        }

        private void SetActiveButton(Button active)
        {
            foreach (var btn in new[] { BtnDash, BtnGames, BtnSales, BtnOrders, BtnAnalytic, BtnSettings })
                btn.Classes.Remove("Active");

            active.Classes.Add("Active");
        }
    }
}
