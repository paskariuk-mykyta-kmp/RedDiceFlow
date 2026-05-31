using System;
using Avalonia;
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

        private static string Res(string key) => Application.Current!.FindResource(key) as string ?? key;

        private async System.Threading.Tasks.Task<bool> OnConfirmCancelOrder(Models.Order? order)
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
    }
}
