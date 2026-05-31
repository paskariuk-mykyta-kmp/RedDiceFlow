using System;
using Avalonia;
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

        private static string Res(string key) => Application.Current!.FindResource(key) as string ?? key;

        private async System.Threading.Tasks.Task<bool> OnConfirmCancelOrder(Order? order)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm == null) return false;

            var dialog = new ConfirmationDialog();
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window owner)
            {
                dialog.Title = Res("L_ConfirmCancelTitle");
                dialog.SetMessage(Res("L_ConfirmCancelMessage"));
                dialog.SetButtonTexts(Res("L_Yes"), Res("L_No"));
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
                dialog.Title = Res("L_ConfirmDeleteTitle");
                dialog.SetMessage(string.Format(Res("L_ConfirmDeleteMessage"), order?.Id));
                dialog.SetButtonTexts(Res("L_Delete"), Res("L_Cancel"));
                return await dialog.ShowDialog(owner);
            }
            return false;
        }
    }
}
