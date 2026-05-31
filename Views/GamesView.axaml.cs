using System.Threading.Tasks;
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

    private async Task<bool> OnConfirmDelete(Models.Product? product)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return false;

        var dialog = new ConfirmationDialog();
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window owner)
        {
            dialog.Title = vm.L_ConfirmDeleteTitle;
            dialog.SetMessage(string.Format(vm.L_ConfirmDeleteMessage, product?.Name));
            dialog.SetButtonTexts(vm.L_Yes, vm.L_No);
            return await dialog.ShowDialog(owner);
        }
        return false;
    }
}
