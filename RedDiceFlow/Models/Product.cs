using CommunityToolkit.Mvvm.ComponentModel;

namespace RedDiceFlow.Models
{
    public partial class Product : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private double _price;

        [ObservableProperty]
        private int _stock;

        [ObservableProperty]
        private string _sku = string.Empty;

        [ObservableProperty]
        private string _genre = string.Empty;

        [ObservableProperty]
        private int _playersCount = 2;

        public string StockStatusText => Stock <= 5 ? "LOW" : "OK";
        public bool IsLowStock => Stock <= 5;

        partial void OnStockChanged(int value)
        {
            OnPropertyChanged(nameof(StockStatusText));
            OnPropertyChanged(nameof(IsLowStock));
        }
    }
}
