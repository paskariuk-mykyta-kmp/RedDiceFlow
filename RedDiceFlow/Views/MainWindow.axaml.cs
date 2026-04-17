using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RedDiceFlow.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnMenuClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            StatusDisplay.Text = "Розділ: " + button.Content;
        }
    }

    private void OnAddRecordClick(object? sender, RoutedEventArgs e)
    {
        string note = QuickNoteInput.Text ?? "";

        if (string.IsNullOrWhiteSpace(note))
        {
            StatusDisplay.Text = "Помилка: Поле порожнє";
        }
        else
        {
            StatusDisplay.Text = "Запис '" + note + "' додано";
            QuickNoteInput.Text = "";
        }
    }
}