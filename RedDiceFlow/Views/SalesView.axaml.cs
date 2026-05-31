using System.Threading.Tasks;
using Avalonia.Controls;
using RedDiceFlow.ViewModels;

namespace RedDiceFlow.Views
{
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ConfirmCancelOrderRequested += OnConfirmCancelOrder;
            }
        }

        private async Task<bool> OnConfirmCancelOrder(Models.Order? order)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm == null) return false;

            var dialog = new ConfirmationDialog();
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window owner)
            {
                dialog.Title = vm.L_ConfirmCancelTitle;
                dialog.SetMessage(vm.L_ConfirmCancelMessage);
                dialog.SetButtonTexts(vm.L_Yes, vm.L_No);
                return await dialog.ShowDialog(owner);
            }
            return false;
        }
    }
}
