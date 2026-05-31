using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace RedDiceFlow.Views;

public partial class ConfirmationDialog : Window
{
    public ConfirmationDialog()
    {
        InitializeComponent();

        YesButton.Click += (_, _) => Close(true);
        NoButton.Click += (_, _) => Close(false);
    }

    public void SetMessage(string message)
    {
        MessageText.Text = message;
    }

    public void SetButtonTexts(string yesText, string noText)
    {
        YesButton.Content = yesText;
        NoButton.Content = noText;
    }

    public Task<bool> ShowDialog(Window owner)
    {
        Owner = owner;
        return ShowDialog<bool>(owner);
    }
}
