using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

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
            {
                var inactiveColor = this.TryFindResource("BorderCol", out var res) && res is Color c
                    ? new SolidColorBrush(c)
                    : new SolidColorBrush(Color.Parse("#252525"));

                btn.Background = inactiveColor;
            }

            var activeColor = this.TryFindResource("MainAccent", out var accent) && accent is Color ac
                ? new SolidColorBrush(ac)
                : new SolidColorBrush(Color.Parse("#C3073F"));

            active.Background = activeColor;
        }
    }
}
