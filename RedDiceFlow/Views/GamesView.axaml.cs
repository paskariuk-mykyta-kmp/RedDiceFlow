using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using RedDiceFlow.ViewModels;

namespace RedDiceFlow.Views;

public partial class GamesView : UserControl
{
    public GamesView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ConfirmDeleteRequested += OnConfirmDelete;
        }
    }

    private static string Res(string key) => Application.Current!.FindResource(key) as string ?? key;

    private async Task<bool> OnConfirmDelete(Models.Product? product)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return false;

        var dialog = new ConfirmationDialog();
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window owner)
        {
            dialog.Title = Res("L_ConfirmDeleteTitle");
            var msg = Res("L_ConfirmDeleteMessage");
            dialog.SetMessage(string.Format(msg, product?.Name));
            dialog.SetButtonTexts(Res("L_Yes"), Res("L_No"));
            return await dialog.ShowDialog(owner);
        }
        return false;
    }
}
