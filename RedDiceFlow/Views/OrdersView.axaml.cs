using System;
using Avalonia.Controls;
using RedDiceFlow.Models;
using RedDiceFlow.ViewModels;

namespace RedDiceFlow.Views
{
    public partial class OrdersView : UserControl
    {
        public OrdersView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ConfirmCancelOrderRequested += OnConfirmCancelOrder;
                vm.ConfirmDeleteOrderRequested += OnConfirmDeleteOrder;
            }
        }

        private async System.Threading.Tasks.Task<bool> OnConfirmCancelOrder(Order? order)
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

        private async System.Threading.Tasks.Task<bool> OnConfirmDeleteOrder(Order? order)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm == null) return false;

            var dialog = new ConfirmationDialog();
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window owner)
            {
                dialog.Title = "Delete Order";
                dialog.SetMessage($"Permanently delete order #{order?.Id}? This will restore stock and remove the order history.");
                dialog.SetButtonTexts("Delete", "Cancel");
                return await dialog.ShowDialog(owner);
            }
            return false;
        }
    }
}
