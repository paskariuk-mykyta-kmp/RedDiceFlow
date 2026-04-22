using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RedDiceFlow.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void OnAddRecordClick(object? sender, RoutedEventArgs e)
    {
        
        QuickNoteInput.Text = "";
    }
}