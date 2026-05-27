using RedDiceFlow.Models;
using RedDiceFlow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace RedDiceFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService = new();
        private readonly ReportService _reportService = new();
        private bool _isUkrainian = false;

        public string L_MenuDashboard { get; set; }
        public string L_MenuInventory { get; set; }
        public string L_MenuSales { get; set; }
        public string L_MenuAnalytics { get; set; }
        public string L_MenuSettings { get; set; }

        public string L_DashWelcome { get; set; }
        public string L_DashSales { get; set; }
        public string L_DashSalesSub { get; set; }
        public string L_DashStock { get; set; }
        public string L_DashStockSub { get; set; }
        public string L_DashStatus { get; set; }
        public string L_DashStatusSub { get; set; }
        public string L_DashQuickActions { get; set; }
        public string L_DashBtnReport { get; set; }
        public string L_DashBtnSync { get; set; }
        public string L_DashBtnUpdate { get; set; }

        public string L_InventoryHeader { get; set; }
        public string L_InventorySub { get; set; }
        public string L_QuickAdd { get; set; }
        public string L_NameLabel { get; set; }
        public string L_SkuLabel { get; set; }
        public string L_PriceLabel { get; set; }
        public string L_StockLabel { get; set; }
        public string L_ConfirmBtn { get; set; }
        public string L_SearchWatermark { get; set; }
        public string L_TableProduct { get; set; }
        public string L_TablePrice { get; set; }
        public string L_TableStock { get; set; }
        public string L_TableStatus { get; set; }
        public string L_TableDel { get; set; }
        public string L_StatusOk { get; set; }
        public string L_SortBtn { get; set; }

        public string L_SettingsHeader { get; set; }
        public string L_SettAccount { get; set; }
        public string L_SettAdmin { get; set; }
        public string L_SettLogout { get; set; }
        public string L_SettGeneral { get; set; }
        public string L_SettLangLabel { get; set; }
        public string L_SettLangCurrent { get; set; }
        public string L_SettChangeBtn { get; set; }
        public string L_SettThemeLabel { get; set; }
        public string L_SettThemeSub { get; set; }
        public string L_SettAuto { get; set; }
        public string L_SettAutoSave { get; set; }
        public string L_SettNotify { get; set; }
        public string L_SettBackup { get; set; }
        public string L_SettSaveAll { get; set; }
        public string L_AdminMode { get; set; }

        public string L_SalesHeader { get; set; }
        public string L_SalesSub { get; set; }
        public string L_RegisterSale { get; set; }
        public string L_SelectProduct { get; set; }
        public string L_Quantity { get; set; }
        public string L_SellButton { get; set; }
        public string L_RecentSales { get; set; }
        public string L_NoSales { get; set; }

        public string L_AnalyticsHeader { get; set; }
        public string L_AnalyticsSub { get; set; }
        public string L_CreatePdf { get; set; }
        public string L_ProductsMetric { get; set; }
        public string L_StockMetric { get; set; }
        public string L_SoldMetric { get; set; }
        public string L_RevenueMetric { get; set; }
        public string L_Infographic { get; set; }
        public string L_StockLoad { get; set; }
        public string L_SalesProgress { get; set; }
        public string L_LowStockRisk { get; set; }
        public string L_LowStock { get; set; }
        public string L_StockValue { get; set; }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isLightTheme = false;
        public bool IsLightTheme
        {
            get => _isLightTheme;
            set
            {
                _isLightTheme = value;
                OnPropertyChanged();
                App.SwitchTheme(value);
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredProducts));
            }
        }

        public string NewName { get; set; } = string.Empty;
        public string NewSku { get; set; } = string.Empty;
        public string NewPrice { get; set; } = "0";
        public string NewStock { get; set; } = "0";
        public string NewGenre { get; set; } = string.Empty;

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Sale> Sales { get; set; }

        private Product? _selectedProductForSale;
        public Product? SelectedProductForSale
        {
            get => _selectedProductForSale;
            set { _selectedProductForSale = value; OnPropertyChanged(); }
        }

        private string _saleQuantity = "1";
        public string SaleQuantity
        {
            get => _saleQuantity;
            set { _saleQuantity = value; OnPropertyChanged(); }
        }

        public IEnumerable<Product> FilteredProducts => string.IsNullOrWhiteSpace(SearchText)
            ? Products
            : Products.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                               || p.Sku.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        public int TotalProducts => Products.Count;
        public int TotalStock => Products.Sum(p => p.Stock);
        public double TotalStockValue => Products.Sum(p => p.Stock * p.Price);
        public int SoldItems => Sales.Sum(s => s.Quantity);
        public double SalesTotal => Sales.Sum(s => s.TotalPrice);
        public int LowStockCount => Products.Count(p => p.Stock <= 5);
        public bool HasNoSales => Sales.Count == 0;
        public IEnumerable<Sale> RecentSales => Sales.Take(8);
        public IEnumerable<Product> LowStockProducts => Products.Where(p => p.Stock <= 5);

        public double StockLoadPercent => Math.Min(100, TotalStock);
        public double SalesProgressPercent => Math.Min(100, SalesTotal / 10);
        public double LowStockRiskPercent => TotalProducts == 0 ? 0 : Math.Round((double)LowStockCount / TotalProducts * 100);

        public MainWindowViewModel()
        {
            Products = new ObservableCollection<Product>(_databaseService.GetProducts());

            if (Products.Count == 0)
                AddDefaultProducts();

            Sales = new ObservableCollection<Sale>(_databaseService.GetSales());

            foreach (var product in Products)
                product.PropertyChanged += OnProductChanged;

            SetLanguage();
        }

        private void AddDefaultProducts()
        {
            var demoProducts = new[]
            {
                new Product { Name = "Catan", Price = 45.00, Stock = 10, Sku = "RD-1011", Genre = "Strategy" },
                new Product { Name = "Dixit", Price = 30.00, Stock = 20, Sku = "RD-5566", Genre = "Party" },
                
            };

            foreach (var product in demoProducts)
            {
                product.Id = _databaseService.AddProduct(product);
                Products.Add(product);
            }
        }

        public void SortByGenre()
        {
            var sortedList = Products.OrderBy(p => p.Genre).ToList();
            Products.Clear();

            foreach (var item in sortedList)
                Products.Add(item);

            OnPropertyChanged(nameof(FilteredProducts));
        }

        public void ToggleLanguage()
        {
            _isUkrainian = !_isUkrainian;
            SetLanguage();
            OnPropertyChanged((string?)null);
        }

        private void SetLanguage()
        {
            if (_isUkrainian)
            {
                L_MenuDashboard = "Головна"; L_MenuInventory = "Інвентар"; L_MenuSales = "Продажі";
                L_MenuAnalytics = "Аналітика"; L_MenuSettings = "Налаштування";

                L_DashWelcome = "Ласкаво просимо"; L_DashSales = "Продажі"; L_DashSalesSub = "Огляд продажів";
                L_DashStock = "Склад"; L_DashStockSub = "Поточні залишки"; L_DashStatus = "Статус";
                L_DashStatusSub = "Стан системи"; L_DashQuickActions = "Швидкі дії"; L_DashBtnReport = "Звіт";
                L_DashBtnSync = "Синхронізація"; L_DashBtnUpdate = "Оновити";

                L_InventoryHeader = "Інвентар товарів"; L_InventorySub = "Керування складом"; L_QuickAdd = "Швидке додавання";
                L_NameLabel = "Назва"; L_SkuLabel = "Артикул"; L_PriceLabel = "Ціна"; L_StockLabel = "Склад";
                L_ConfirmBtn = "Додати"; L_SearchWatermark = "Пошук..."; L_TableProduct = "ТОВАР";
                L_TablePrice = "ЦІНА"; L_TableStock = "ЗАЛИШОК"; L_TableStatus = "СТАТУС"; L_TableDel = "ВИД.";
                L_StatusOk = "OK"; L_SortBtn = "Сортувати за жанром";

                L_SettingsHeader = "Налаштування"; L_SettAccount = "Акаунт"; L_SettAdmin = "Адмін"; L_SettLogout = "Вийти";
                L_SettGeneral = "Загальні"; L_SettLangLabel = "Мова"; L_SettLangCurrent = "Українська"; L_SettChangeBtn = "Змінити";
                L_SettThemeLabel = "Тема"; L_SettThemeSub = "Оберіть тему"; L_SettAuto = "Автоматизація";
                L_SettAutoSave = "Автозбереження"; L_SettNotify = "Сповіщення"; L_SettBackup = "Резервна копія";
                L_SettSaveAll = "Зберегти все"; L_AdminMode = "Режим Адміна";

                L_SalesHeader = "Продажі"; L_SalesSub = "Реєстрація продажів настільних ігор";
                L_RegisterSale = "Оформити продаж"; L_SelectProduct = "Оберіть гру";
                L_Quantity = "К-сть"; L_SellButton = "Продати"; L_RecentSales = "Останні продажі";
                L_NoSales = "Продажів поки немає";

                L_AnalyticsHeader = "Аналітика"; L_AnalyticsSub = "Жива статистика складу та продажів";
                L_CreatePdf = "Створити PDF-звіт"; L_ProductsMetric = "Товарів";
                L_StockMetric = "На складі"; L_SoldMetric = "Продано"; L_RevenueMetric = "Виручка";
                L_Infographic = "Інфографіка"; L_StockLoad = "Заповнення складу";
                L_SalesProgress = "Прогрес продажів"; L_LowStockRisk = "Ризик дефіциту";
                L_LowStock = "Мало на складі"; L_StockValue = "Вартість складу";

                StatusMessage = "Система онлайн";
            }
            else
            {
                L_MenuDashboard = "Dashboard"; L_MenuInventory = "Inventory"; L_MenuSales = "Sales";
                L_MenuAnalytics = "Analytics"; L_MenuSettings = "Settings";

                L_DashWelcome = "Welcome"; L_DashSales = "Sales"; L_DashSalesSub = "Sales overview";
                L_DashStock = "Stock"; L_DashStockSub = "Current inventory"; L_DashStatus = "Status";
                L_DashStatusSub = "System state"; L_DashQuickActions = "Quick Actions"; L_DashBtnReport = "Report";
                L_DashBtnSync = "Sync"; L_DashBtnUpdate = "Update";

                L_InventoryHeader = "Product Inventory"; L_InventorySub = "Manage stock"; L_QuickAdd = "Quick Add";
                L_NameLabel = "Name"; L_SkuLabel = "SKU"; L_PriceLabel = "Price"; L_StockLabel = "Stock";
                L_ConfirmBtn = "Add"; L_SearchWatermark = "Search..."; L_TableProduct = "PRODUCT";
                L_TablePrice = "PRICE"; L_TableStock = "STOCK"; L_TableStatus = "STATUS"; L_TableDel = "DEL";
                L_StatusOk = "OK"; L_SortBtn = "Sort by Genre";

                L_SettingsHeader = "Settings"; L_SettAccount = "Account"; L_SettAdmin = "Admin"; L_SettLogout = "Logout";
                L_SettGeneral = "General"; L_SettLangLabel = "Language"; L_SettLangCurrent = "English"; L_SettChangeBtn = "Change";
                L_SettThemeLabel = "Theme"; L_SettThemeSub = "Choose theme"; L_SettAuto = "Automation";
                L_SettAutoSave = "Auto Save"; L_SettNotify = "Notifications"; L_SettBackup = "Backup";
                L_SettSaveAll = "Save All"; L_AdminMode = "Admin Mode";

                L_SalesHeader = "Sales"; L_SalesSub = "Register board game sales";
                L_RegisterSale = "Register sale"; L_SelectProduct = "Select product";
                L_Quantity = "Qty"; L_SellButton = "Sell"; L_RecentSales = "Recent sales";
                L_NoSales = "No sales yet";

                L_AnalyticsHeader = "Analytics"; L_AnalyticsSub = "Live inventory and sales statistics";
                L_CreatePdf = "Create PDF report"; L_ProductsMetric = "Products";
                L_StockMetric = "In stock"; L_SoldMetric = "Sold"; L_RevenueMetric = "Revenue";
                L_Infographic = "Infographic"; L_StockLoad = "Stock load";
                L_SalesProgress = "Sales progress"; L_LowStockRisk = "Low-stock risk";
                L_LowStock = "Low stock"; L_StockValue = "Stock value";

                StatusMessage = "System Online";
            }
        }

        public void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(NewName))
                return;

            if (!double.TryParse(NewPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            {
                StatusMessage = "Invalid price";
                return;
            }

            if (!int.TryParse(NewStock, out var stock))
            {
                StatusMessage = "Invalid stock";
                return;
            }

            var product = new Product
            {
                Name = NewName,
                Sku = NewSku,
                Price = price,
                Stock = stock,
                Genre = NewGenre
            };

            product.Id = _databaseService.AddProduct(product);
            product.PropertyChanged += OnProductChanged;
            Products.Add(product);

            NewName = string.Empty;
            NewSku = string.Empty;
            NewPrice = "0";
            NewStock = "0";
            NewGenre = string.Empty;

            OnPropertyChanged((string?)null);
            RefreshAnalytics();
        }

        public void RemoveProduct(Product product)
        {
            if (product == null)
                return;

            product.PropertyChanged -= OnProductChanged;
            _databaseService.DeleteProduct(product.Id);
            Products.Remove(product);

            OnPropertyChanged(nameof(FilteredProducts));
            RefreshAnalytics();
        }

        public void SellSelectedProduct()
        {
            if (SelectedProductForSale == null)
            {
                StatusMessage = _isUkrainian ? "Оберіть товар" : "Select product first";
                return;
            }

            if (!int.TryParse(SaleQuantity, out var quantity) || quantity <= 0)
            {
                StatusMessage = _isUkrainian ? "Некоректна кількість" : "Invalid quantity";
                return;
            }

            if (SelectedProductForSale.Stock < quantity)
            {
                StatusMessage = _isUkrainian ? "Недостатньо товару на складі" : "Not enough stock";
                return;
            }

            try
            {
                _databaseService.AddSale(SelectedProductForSale, quantity);
                SelectedProductForSale.Stock -= quantity;

                Sales.Clear();
                foreach (var sale in _databaseService.GetSales())
                    Sales.Add(sale);

                SaleQuantity = "1";
                StatusMessage = _isUkrainian ? "Продаж збережено" : "Sale saved";
                RefreshAnalytics();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public void CreateManagerReport()
        {
            var path = _reportService.CreateManagerReport(Products, Sales);
            StatusMessage = _isUkrainian ? $"Звіт створено: {path}" : $"Report created: {path}";
        }

        private void RefreshAnalytics()
        {
            OnPropertyChanged(nameof(TotalProducts));
            OnPropertyChanged(nameof(TotalStock));
            OnPropertyChanged(nameof(TotalStockValue));
            OnPropertyChanged(nameof(SoldItems));
            OnPropertyChanged(nameof(SalesTotal));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(HasNoSales));
            OnPropertyChanged(nameof(RecentSales));
            OnPropertyChanged(nameof(LowStockProducts));
            OnPropertyChanged(nameof(StockLoadPercent));
            OnPropertyChanged(nameof(SalesProgressPercent));
            OnPropertyChanged(nameof(LowStockRiskPercent));
            OnPropertyChanged(nameof(FilteredProducts));
        }

        private void OnProductChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Product product)
            {
                _databaseService.UpdateProduct(product);
                RefreshAnalytics();
            }
        }
    }
}