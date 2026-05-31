using CommunityToolkit.Mvvm.Input;
using RedDiceFlow.Models;
using RedDiceFlow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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
        public string L_MenuOrders { get; set; }
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
        public string L_StatusLow { get; set; }
        public string L_SortBtn { get; set; }
        public string L_TablePlayers { get; set; }
        public string L_TableActions { get; set; }
        public string L_EditBtn { get; set; }
        public string L_RestockBtn { get; set; }
        public string L_DeleteBtn { get; set; }
        public string L_GenreLabel { get; set; }
        public string L_PlayersLabel { get; set; }
        public string L_SaveBtn { get; set; }
        public string L_CancelBtn { get; set; }

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
        public string L_CartHeader { get; set; }
        public string L_SearchProductPlaceholder { get; set; }
        public string L_AddToCart { get; set; }
        public string L_QuantityLabel { get; set; }
        public string L_RemoveFromCart { get; set; }
        public string L_CartTotal { get; set; }
        public string L_CartEmpty { get; set; }
        public string L_PlaceOrder { get; set; }
        public string L_CustomerSearchLabel { get; set; }
        public string L_CustomerFound { get; set; }
        public string L_CustomerNewName { get; set; }
        public string L_RecentOrders { get; set; }
        public string L_NoRecentOrders { get; set; }
        public string L_CancelOrder { get; set; }
        public string L_OrderPlaced { get; set; }
        public string L_OrderCancelled { get; set; }

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

        public string L_OrdersHeader { get; set; }
        public string L_OrdersSub { get; set; }
        public string L_OrderSearch { get; set; }
        public string L_OrderSearchButton { get; set; }
        public string L_OrderList { get; set; }
        public string L_OrderDetails { get; set; }
        public string L_OrderNoSelection { get; set; }
        public string L_OrderItemsCount { get; set; }
        public string L_OrderCustomer { get; set; }
        public string L_OrderDate { get; set; }
        public string L_OrderTotal { get; set; }

        public string L_ConfirmDeleteTitle { get; set; }
        public string L_ConfirmDeleteMessage { get; set; }
        public string L_ConfirmCancelTitle { get; set; }
        public string L_ConfirmCancelMessage { get; set; }
        public string L_Yes { get; set; }
        public string L_No { get; set; }

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
        public string NewPlayersCount { get; set; } = "2";
        private int ParsedNewPlayersCount => int.TryParse(NewPlayersCount, out var pc) ? pc : 2;

        public string[] SortOptions { get; } = new[] { "Name", "Price", "Stock", "Genre" };
        public string[] GenreOptions { get; } = new[] { "Strategy", "Party", "Abstract", "Thematic", "Card Game", "Family", "Euro", "Wargame", "Cooperative" };
        public string[] PlayersCountOptions { get; } = new[] { "1", "2", "3", "4", "5", "6", "7", "8" };

        private bool _isEditMode;
        public bool IsEditMode { get => _isEditMode; set { _isEditMode = value; OnPropertyChanged(); } }

        private Product? _editTargetProduct;
        public Product? EditTargetProduct { get => _editTargetProduct; set { _editTargetProduct = value; OnPropertyChanged(); } }

        private string _sortMode = "Name";
        public string SortMode
        {
            get => _sortMode;
            set { _sortMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredProducts)); }
        }

        public event Func<Product?, Task<bool>>? ConfirmDeleteRequested;
        public event Func<Order?, Task<bool>>? ConfirmCancelOrderRequested;

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Order> Orders { get; set; }
        public ObservableCollection<OrderItem> SelectedOrderItems { get; set; } = new();
        public ObservableCollection<OrderItem> CartItems { get; set; } = new();
        public ObservableCollection<Customer> FoundCustomers { get; set; } = new();

        private string _orderSearchPhone = string.Empty;
        public string OrderSearchPhone
        {
            get => _orderSearchPhone;
            set
            {
                _orderSearchPhone = value;
                OnPropertyChanged();
            }
        }

        private Order? _selectedOrder;
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoSelectedOrder));
                LoadSelectedOrderItems();
            }
        }

        public bool HasNoSelectedOrder => SelectedOrder == null;

        private string _saleSearchText = string.Empty;
        public string SaleSearchText
        {
            get => _saleSearchText;
            set
            {
                _saleSearchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredProductsForSale));
            }
        }

        private string _cartQuantity = "1";
        public string CartQuantity
        {
            get => _cartQuantity;
            set { _cartQuantity = value; OnPropertyChanged(); }
        }

        private Product? _selectedProductForCart;
        public Product? SelectedProductForCart
        {
            get => _selectedProductForCart;
            set { _selectedProductForCart = value; OnPropertyChanged(); }
        }

        private bool _isUpdatingCustomer;
        private string _customerSearchText = string.Empty;
        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                if (_customerSearchText == value)
                    return;
                _customerSearchText = value;
                OnPropertyChanged();
                if (!_isUpdatingCustomer)
                    SearchCustomer();
            }
        }

        private Customer? _selectedFoundCustomer;
        public Customer? SelectedFoundCustomer
        {
            get => _selectedFoundCustomer;
            set
            {
                if (value?.Id == _selectedFoundCustomer?.Id && value != null)
                    return;
                _selectedFoundCustomer = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _isUpdatingCustomer = true;
                    CustomerSearchText = value.Phone;
                    _isUpdatingCustomer = false;
                    NewCustomerName = value.FullName;
                }
                OnPropertyChanged(nameof(HasFoundCustomer));
                OnPropertyChanged(nameof(ShowNewCustomerField));
            }
        }

        public bool HasFoundCustomer => SelectedFoundCustomer != null;
        public bool HasCartItems => CartItems.Count > 0;
        public bool IsCartEmpty => CartItems.Count == 0;
        public bool ShowNewCustomerField => !HasFoundCustomer;

        private string _newCustomerName = string.Empty;
        public string NewCustomerName
        {
            get => _newCustomerName;
            set { _newCustomerName = value; OnPropertyChanged(); }
        }

        public double CartTotalPrice => CartItems.Sum(i => i.LineTotal);

        public IEnumerable<Product> FilteredProducts
        {
            get
            {
                var query = string.IsNullOrWhiteSpace(SearchText)
                    ? Products
                    : Products.Where(p =>
                           p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                        || p.Sku.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                        || p.Genre.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                return SortMode switch
                {
                    "Price" => query.OrderBy(p => p.Price),
                    "Stock" => query.OrderBy(p => p.Stock),
                    "Genre" => query.OrderBy(p => p.Genre).ThenBy(p => p.Name),
                    _ => query.OrderBy(p => p.Name),
                };
            }
        }

        public IEnumerable<Product> FilteredProductsForSale => string.IsNullOrWhiteSpace(SaleSearchText)
            ? Products
            : Products.Where(p => p.Name.Contains(SaleSearchText, StringComparison.OrdinalIgnoreCase)
                               || p.Sku.Contains(SaleSearchText, StringComparison.OrdinalIgnoreCase)
                               || p.Genre.Contains(SaleSearchText, StringComparison.OrdinalIgnoreCase));

        public int TotalProducts => Products.Count;
        public int TotalStock => Products.Sum(p => p.Stock);
        public double TotalStockValue => Products.Sum(p => p.Stock * p.Price);
        public int LowStockCount => Products.Count(p => p.Stock <= 5);
        public int SoldItems => Orders.Sum(o => o.ItemsCount);
        public double SalesTotal => Orders.Sum(o => o.TotalPrice);
        public bool HasNoRecentOrders => Orders.Count == 0;
        public IEnumerable<Order> RecentOrders => Orders.Take(10);
        public IEnumerable<Product> LowStockProducts => Products.Where(p => p.Stock <= 5);

        public double StockLoadPercent => Math.Min(100, TotalStock);
        public double SalesProgressPercent => Math.Min(100, SalesTotal / 10);
        public double LowStockRiskPercent => TotalProducts == 0 ? 0 : Math.Round((double)LowStockCount / TotalProducts * 100);

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand SearchOrdersCommand { get; }
        public ICommand ReloadOrdersCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand RestockProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand CancelEditCommand { get; }

        public MainWindowViewModel()
        {
            Products = new ObservableCollection<Product>(_databaseService.GetProducts());

            if (Products.Count == 0)
                AddDefaultProducts();

            Orders = new ObservableCollection<Order>(_databaseService.GetOrders());
            UpdateDisplayNumbers();

            foreach (var product in Products)
                product.PropertyChanged += OnProductChanged;

            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            RemoveFromCartCommand = new RelayCommand<OrderItem>(RemoveFromCart);
            PlaceOrderCommand = new RelayCommand(PlaceOrder);
            CancelOrderCommand = new RelayCommand<Order>(CancelOrder);
            SearchOrdersCommand = new RelayCommand(SearchOrders);
            ReloadOrdersCommand = new RelayCommand(ReloadOrders);
            EditProductCommand = new RelayCommand<Product>(EditProduct);
            RestockProductCommand = new RelayCommand<Product>(RestockProduct);
            DeleteProductCommand = new RelayCommand<Product>(DeleteProduct);
            CancelEditCommand = new RelayCommand(CancelEdit);

            SetLanguage();
        }

        private void AddDefaultProducts()
        {
            var demoProducts = new[]
            {
                new Product { Name = "Catan", Price = 45.00, Stock = 10, Sku = "RD-1011", Genre = "Strategy", PlayersCount = 3 },
                new Product { Name = "Dixit", Price = 30.00, Stock = 20, Sku = "RD-5566", Genre = "Party", PlayersCount = 6 },
                new Product { Name = "Ticket to Ride", Price = 50.00, Stock = 5, Sku = "RD-2020", Genre = "Strategy", PlayersCount = 5 },
                new Product { Name = "Codenames", Price = 20.00, Stock = 15, Sku = "RD-3030", Genre = "Party", PlayersCount = 8 },
                new Product { Name = "Azul", Price = 35.00, Stock = 2, Sku = "RD-4040", Genre = "Abstract", PlayersCount = 4 },
            };

            foreach (var product in demoProducts)
            {
                product.Id = _databaseService.AddProduct(product);
                Products.Add(product);
            }
        }

        public void SortBy(string mode)
        {
            SortMode = mode;
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
                L_MenuDashboard = "Головна"; L_MenuInventory = "Інвентар"; L_MenuSales = "Продажі"; L_MenuOrders = "Замовлення";
                L_MenuAnalytics = "Аналітика"; L_MenuSettings = "Налаштування";

                L_DashWelcome = "Ласкаво просимо"; L_DashSales = "Продажі"; L_DashSalesSub = "Огляд продажів";
                L_DashStock = "Склад"; L_DashStockSub = "Поточні залишки"; L_DashStatus = "Статус";
                L_DashStatusSub = "Стан системи"; L_DashQuickActions = "Швидкі дії"; L_DashBtnReport = "Звіт";
                L_DashBtnSync = "Синхронізація"; L_DashBtnUpdate = "Оновити";

                L_InventoryHeader = "Інвентар товарів"; L_InventorySub = "Керування складом"; L_QuickAdd = "Швидке додавання";
                L_NameLabel = "Назва"; L_SkuLabel = "Артикул"; L_PriceLabel = "Ціна"; L_StockLabel = "Склад";
                L_ConfirmBtn = "Додати"; L_SearchWatermark = "Пошук..."; L_TableProduct = "ТОВАР";
                L_TablePrice = "ЦІНА"; L_TableStock = "ЗАЛИШОК"; L_TableStatus = "СТАТУС"; L_TableDel = "ВИД.";
                L_StatusOk = "OK"; L_StatusLow = "МАЛО"; L_SortBtn = "Сортувати за жанром";
                L_TablePlayers = "ГРАВЦІ"; L_TableActions = "ДІЇ";
                L_EditBtn = "✎"; L_RestockBtn = "+1"; L_DeleteBtn = "✕";
                L_GenreLabel = "Жанр"; L_PlayersLabel = "Гравці";
                L_SaveBtn = "Зберегти"; L_CancelBtn = "Скасувати";

                L_SettingsHeader = "Налаштування"; L_SettAccount = "Акаунт"; L_SettAdmin = "Адмін"; L_SettLogout = "Вийти";
                L_SettGeneral = "Загальні"; L_SettLangLabel = "Мова"; L_SettLangCurrent = "Українська"; L_SettChangeBtn = "Змінити";
                L_SettThemeLabel = "Тема"; L_SettThemeSub = "Оберіть тему"; L_SettAuto = "Автоматизація";
                L_SettAutoSave = "Автозбереження"; L_SettNotify = "Сповіщення"; L_SettBackup = "Резервна копія";
                L_SettSaveAll = "Зберегти все"; L_AdminMode = "Режим Адміна";

                L_SalesHeader = "Продажі"; L_SalesSub = "Реєстрація продажів настільних ігор";
                L_CartHeader = "Кошик"; L_SearchProductPlaceholder = "Пошук товару (назва, артикул, жанр...)";
                L_AddToCart = "Додати"; L_QuantityLabel = "К-сть"; L_RemoveFromCart = "✕";
                L_CartTotal = "Загальна сума"; L_CartEmpty = "Кошик порожній";
                L_PlaceOrder = " Оформити замовлення";
                L_CustomerSearchLabel = "Телефон або ПІБ клієнта";
                L_CustomerFound = "Знайдено:"; L_CustomerNewName = "Ім'я нового клієнта";
                L_RecentOrders = "Останні замовлення"; L_NoRecentOrders = "Замовлень поки немає";
                L_CancelOrder = "✕ Скасувати";
                L_OrderPlaced = " Замовлення оформлено"; L_OrderCancelled = " Замовлення скасовано, залишки повернено";

                L_AnalyticsHeader = "Аналітика"; L_AnalyticsSub = "Жива статистика складу та продажів";
                L_CreatePdf = "Створити PDF-звіт"; L_ProductsMetric = "Товарів";
                L_StockMetric = "На складі"; L_SoldMetric = "Продано"; L_RevenueMetric = "Виручка";
                L_Infographic = "Інфографіка"; L_StockLoad = "Заповнення складу";
                L_SalesProgress = "Прогрес продажів"; L_LowStockRisk = "Ризик дефіциту";
                L_LowStock = "Мало на складі"; L_StockValue = "Вартість складу";

                L_OrdersHeader = "Замовлення";
                L_OrdersSub = "Історія замовлень та пошук за номером клієнта";
                L_OrderSearch = "Номер клієнта";
                L_OrderSearchButton = "Шукати";
                L_OrderList = "Список замовлень";
                L_OrderDetails = "Склад замовлення";
                L_OrderNoSelection = "Оберіть замовлення зі списку";
                L_OrderItemsCount = "Позицій";
                L_OrderCustomer = "Клієнт";
                L_OrderDate = "Дата";
                L_OrderTotal = "Сума";

                L_ConfirmDeleteTitle = "Підтвердження";
                L_ConfirmDeleteMessage = "Ви впевнені, що хочете видалити \"{0}\"?";
                L_ConfirmCancelTitle = "Підтвердження";
                L_ConfirmCancelMessage = "Скасувати це замовлення?";
                L_Yes = "Так";
                L_No = "Ні";

                StatusMessage = "Система онлайн";
            }
            else
            {
                L_MenuDashboard = "Dashboard"; L_MenuInventory = "Inventory"; L_MenuSales = "Sales"; L_MenuOrders = "Orders";
                L_MenuAnalytics = "Analytics"; L_MenuSettings = "Settings";

                L_DashWelcome = "Welcome"; L_DashSales = "Sales"; L_DashSalesSub = "Sales overview";
                L_DashStock = "Stock"; L_DashStockSub = "Current inventory"; L_DashStatus = "Status";
                L_DashStatusSub = "System state"; L_DashQuickActions = "Quick Actions"; L_DashBtnReport = "Report";
                L_DashBtnSync = "Sync"; L_DashBtnUpdate = "Update";

                L_InventoryHeader = "Product Inventory"; L_InventorySub = "Manage stock"; L_QuickAdd = "Quick Add";
                L_NameLabel = "Name"; L_SkuLabel = "SKU"; L_PriceLabel = "Price"; L_StockLabel = "Stock";
                L_ConfirmBtn = "Add"; L_SearchWatermark = "Search..."; L_TableProduct = "PRODUCT";
                L_TablePrice = "PRICE"; L_TableStock = "STOCK"; L_TableStatus = "STATUS"; L_TableDel = "DEL";
                L_StatusOk = "OK"; L_StatusLow = "LOW"; L_SortBtn = "Sort by Genre";
                L_TablePlayers = "PLAYERS"; L_TableActions = "ACTIONS";
                L_EditBtn = "✎"; L_RestockBtn = "+1"; L_DeleteBtn = "✕";
                L_GenreLabel = "Genre"; L_PlayersLabel = "Players";
                L_SaveBtn = "Save"; L_CancelBtn = "Cancel";

                L_SettingsHeader = "Settings"; L_SettAccount = "Account"; L_SettAdmin = "Admin"; L_SettLogout = "Logout";
                L_SettGeneral = "General"; L_SettLangLabel = "Language"; L_SettLangCurrent = "English"; L_SettChangeBtn = "Change";
                L_SettThemeLabel = "Theme"; L_SettThemeSub = "Choose theme"; L_SettAuto = "Automation";
                L_SettAutoSave = "Auto Save"; L_SettNotify = "Notifications"; L_SettBackup = "Backup";
                L_SettSaveAll = "Save All"; L_AdminMode = "Admin Mode";

                L_SalesHeader = "Sales"; L_SalesSub = "Register board game sales";
                L_CartHeader = "Cart"; L_SearchProductPlaceholder = "Search product (name, SKU, genre...)";
                L_AddToCart = "Add"; L_QuantityLabel = "Qty"; L_RemoveFromCart = "✕";
                L_CartTotal = "Total"; L_CartEmpty = "Cart is empty";
                L_PlaceOrder = " Place Order";
                L_CustomerSearchLabel = "Customer phone or name";
                L_CustomerFound = "Found:"; L_CustomerNewName = "New customer name";
                L_RecentOrders = "Recent orders"; L_NoRecentOrders = "No orders yet";
                L_CancelOrder = "✕ Cancel";
                L_OrderPlaced = "Order placed successfully"; L_OrderCancelled = "Order cancelled, stock restored";

                L_AnalyticsHeader = "Analytics"; L_AnalyticsSub = "Live inventory and sales statistics";
                L_CreatePdf = "Create PDF report"; L_ProductsMetric = "Products";
                L_StockMetric = "In stock"; L_SoldMetric = "Sold"; L_RevenueMetric = "Revenue";
                L_Infographic = "Infographic"; L_StockLoad = "Stock load";
                L_SalesProgress = "Sales progress"; L_LowStockRisk = "Low-stock risk";
                L_LowStock = "Low stock"; L_StockValue = "Stock value";

                L_OrdersHeader = "Orders";
                L_OrdersSub = "Order history and customer phone search";
                L_OrderSearch = "Customer phone";
                L_OrderSearchButton = "Search";
                L_OrderList = "Order list";
                L_OrderDetails = "Order items";
                L_OrderNoSelection = "Select an order from the list";
                L_OrderItemsCount = "Items";
                L_OrderCustomer = "Customer";
                L_OrderDate = "Date";
                L_OrderTotal = "Total";

                L_ConfirmDeleteTitle = "Confirm";
                L_ConfirmDeleteMessage = "Are you sure you want to delete \"{0}\"?";
                L_ConfirmCancelTitle = "Confirm";
                L_ConfirmCancelMessage = "Cancel this order?";
                L_Yes = "Yes";
                L_No = "No";

                StatusMessage = "System Online";
            }
        }

        public void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(NewName))
            {
                StatusMessage = _isUkrainian ? "Введіть назву" : "Enter name";
                return;
            }

            if (!double.TryParse(NewPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
            {
                StatusMessage = _isUkrainian ? "Некоректна ціна" : "Invalid price";
                return;
            }

            if (!int.TryParse(NewStock, out var stock) || stock < 0)
            {
                StatusMessage = _isUkrainian ? "Некоректна кількість" : "Invalid stock";
                return;
            }

            if (IsEditMode && EditTargetProduct != null)
            {
                EditTargetProduct.Name = NewName;
                EditTargetProduct.Sku = NewSku;
                EditTargetProduct.Price = price;
                EditTargetProduct.Stock = stock;
                EditTargetProduct.Genre = NewGenre;
                EditTargetProduct.PlayersCount = ParsedNewPlayersCount;
                _databaseService.UpdateProduct(EditTargetProduct);

                var idx = Products.IndexOf(EditTargetProduct);
                if (idx >= 0)
                {
                    Products.RemoveAt(idx);
                    EditTargetProduct.PropertyChanged += OnProductChanged;
                    Products.Insert(idx, EditTargetProduct);
                }

                CancelEdit();
                RefreshAnalytics();
                return;
            }

            var product = new Product
            {
                Name = NewName,
                Sku = NewSku,
                Price = price,
                Stock = stock,
                Genre = NewGenre,
                PlayersCount = ParsedNewPlayersCount
            };

            product.Id = _databaseService.AddProduct(product);
            product.PropertyChanged += OnProductChanged;
            Products.Add(product);

            NewName = string.Empty;
            NewSku = string.Empty;
            NewPrice = "0";
            NewStock = "0";
            NewGenre = string.Empty;
            NewPlayersCount = "2";

            OnPropertyChanged((string?)null);
            RefreshAnalytics();
        }

        public async void DeleteProduct(Product? product)
        {
            if (product == null)
                return;

            if (ConfirmDeleteRequested != null)
            {
                var confirmed = await ConfirmDeleteRequested(product);
                if (!confirmed)
                    return;
            }

            try
            {
                product.PropertyChanged -= OnProductChanged;
                _databaseService.DeleteProduct(product.Id);
                Products.Remove(product);

                if (EditTargetProduct?.Id == product.Id)
                    CancelEdit();

                OnPropertyChanged(nameof(FilteredProducts));
                RefreshAnalytics();
                StatusMessage = _isUkrainian ? "Товар видалено" : "Product deleted";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public void EditProduct(Product? product)
        {
            if (product == null)
                return;

            IsEditMode = true;
            EditTargetProduct = product;
            NewName = product.Name;
            NewSku = product.Sku;
            NewPrice = product.Price.ToString("F2", CultureInfo.InvariantCulture);
            NewStock = product.Stock.ToString();
            NewGenre = product.Genre ?? string.Empty;
            NewPlayersCount = product.PlayersCount.ToString();
        }

        public void RestockProduct(Product? product)
        {
            if (product == null)
                return;

            product.Stock += 1;
            _databaseService.UpdateProduct(product);
            StatusMessage = _isUkrainian ? $"Додано 1 шт до {product.Name}" : $"Added 1 to {product.Name}";
            RefreshAnalytics();
        }

        public void CancelEdit()
        {
            IsEditMode = false;
            EditTargetProduct = null;
            NewName = string.Empty;
            NewSku = string.Empty;
            NewPrice = "0";
            NewStock = "0";
            NewGenre = string.Empty;
            NewPlayersCount = "2";
            OnPropertyChanged((string?)null);
        }

        public void AddToCart(Product product)
        {
            if (product == null)
            {
                StatusMessage = _isUkrainian ? "Оберіть товар" : "Select product first";
                return;
            }

            if (!int.TryParse(CartQuantity, out var quantity) || quantity <= 0)
            {
                StatusMessage = _isUkrainian ? "Некоректна кількість" : "Invalid quantity";
                return;
            }

            if (product.Stock < quantity)
            {
                StatusMessage = _isUkrainian ? "Недостатньо товару на складі" : "Not enough stock";
                return;
            }

            var existing = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
            if (existing != null)
            {
                var newQty = existing.Quantity + quantity;
                if (product.Stock < newQty)
                {
                    StatusMessage = _isUkrainian ? "Недостатньо товару на складі" : "Not enough stock";
                    return;
                }
                existing.Quantity = newQty;
                existing.LineTotal = existing.UnitPrice * newQty;
                CartItems.Remove(existing);
                CartItems.Add(existing);
            }
            else
            {
                CartItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    Genre = product.Genre,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    LineTotal = product.Price * quantity
                });
            }

            CartQuantity = "1";
            OnPropertyChanged(nameof(CartTotalPrice));
            OnPropertyChanged(nameof(CartItems));
            OnPropertyChanged(nameof(HasCartItems));
            OnPropertyChanged(nameof(IsCartEmpty));
            StatusMessage = _isUkrainian ? "Товар додано в кошик" : "Product added to cart";
        }

        public void RemoveFromCart(OrderItem? item)
        {
            if (item == null)
                return;

            CartItems.Remove(item);
            OnPropertyChanged(nameof(CartTotalPrice));
            OnPropertyChanged(nameof(CartItems));
            OnPropertyChanged(nameof(HasCartItems));
            OnPropertyChanged(nameof(IsCartEmpty));
        }

        public void SelectCustomer(Customer customer)
        {
            SelectedFoundCustomer = customer;
            CustomerSearchText = customer.Phone;
            NewCustomerName = customer.FullName;
        }

        public void SearchCustomer()
        {
            FoundCustomers.Clear();

            if (string.IsNullOrWhiteSpace(CustomerSearchText))
            {
                SelectedFoundCustomer = null;
                NewCustomerName = string.Empty;
                OnPropertyChanged(nameof(HasFoundCustomer));
                return;
            }

            var results = _databaseService.SearchCustomersFuzzy(CustomerSearchText);
            foreach (var c in results)
                FoundCustomers.Add(c);

            if (FoundCustomers.Count > 0)
            {
                SelectedFoundCustomer = FoundCustomers[0];
            }
            else
            {
                SelectedFoundCustomer = null;
                OnPropertyChanged(nameof(HasFoundCustomer));
            }
        }

        public void PlaceOrder()
        {
            if (CartItems.Count == 0)
            {
                StatusMessage = _isUkrainian ? "Кошик порожній" : "Cart is empty";
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerSearchText))
            {
                StatusMessage = _isUkrainian ? "Введіть номер телефону клієнта" : "Enter customer phone";
                return;
            }

            try
            {
                var phone = CustomerSearchText.Trim();
                var name = string.IsNullOrWhiteSpace(NewCustomerName) ? null : NewCustomerName.Trim();
                var items = CartItems.ToList();

                _databaseService.AddOrder(phone, items, name);

                CartItems.Clear();
                CustomerSearchText = string.Empty;
                NewCustomerName = string.Empty;
                SelectedFoundCustomer = null;
                OnPropertyChanged(nameof(CartTotalPrice));
                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(HasCartItems));
                OnPropertyChanged(nameof(IsCartEmpty));
                OnPropertyChanged(nameof(HasFoundCustomer));
                OnPropertyChanged(nameof(ShowNewCustomerField));

                RefreshRecentOrders();
                StatusMessage = _isUkrainian ? L_OrderPlaced : "Order placed successfully";
                RefreshAnalytics();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public async void CancelOrder(Order? order)
        {
            if (order == null)
                return;

            if (ConfirmCancelOrderRequested != null)
            {
                var confirmed = await ConfirmCancelOrderRequested(order);
                if (!confirmed)
                    return;
            }

            try
            {
                _databaseService.CancelOrder(order.Id);
                RefreshRecentOrders();
                RefreshAnalytics();
                StatusMessage = _isUkrainian ? L_OrderCancelled : "Order cancelled and stock restored";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public void RefreshRecentOrders()
        {
            Orders.Clear();
            foreach (var order in _databaseService.GetOrders())
                Orders.Add(order);

            UpdateDisplayNumbers();
            OnPropertyChanged(nameof(RecentOrders));
            OnPropertyChanged(nameof(HasNoRecentOrders));
        }

        private void UpdateDisplayNumbers()
        {
            for (int i = 0; i < Orders.Count; i++)
                Orders[i].DisplayNumber = i + 1;
        }

        public void CreateManagerReport()
        {
            var path = _reportService.CreateManagerReport(Products, Orders);
            StatusMessage = _isUkrainian ? $"Звіт створено: {path}" : $"Report created: {path}";
        }

        public void SearchOrders()
        {
            Orders.Clear();

            var orders = string.IsNullOrWhiteSpace(OrderSearchPhone)
                ? _databaseService.GetOrders()
                : _databaseService.SearchOrdersByCustomerPhone(OrderSearchPhone);

            foreach (var order in orders)
                Orders.Add(order);

            UpdateDisplayNumbers();
            SelectedOrder = Orders.FirstOrDefault();
            OnPropertyChanged(nameof(RecentOrders));
            OnPropertyChanged(nameof(HasNoRecentOrders));
        }

        public void ReloadOrders()
        {
            OrderSearchPhone = string.Empty;
            SearchOrders();
        }

        private void LoadSelectedOrderItems()
        {
            SelectedOrderItems.Clear();

            if (SelectedOrder == null)
                return;

            foreach (var item in _databaseService.GetOrderItems(SelectedOrder.Id))
                SelectedOrderItems.Add(item);
        }

        private void RefreshAnalytics()
        {
            OnPropertyChanged(nameof(TotalProducts));
            OnPropertyChanged(nameof(TotalStock));
            OnPropertyChanged(nameof(TotalStockValue));
            OnPropertyChanged(nameof(SoldItems));
            OnPropertyChanged(nameof(SalesTotal));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(HasNoRecentOrders));
            OnPropertyChanged(nameof(RecentOrders));
            OnPropertyChanged(nameof(LowStockProducts));
            OnPropertyChanged(nameof(StockLoadPercent));
            OnPropertyChanged(nameof(SalesProgressPercent));
            OnPropertyChanged(nameof(LowStockRiskPercent));
            OnPropertyChanged(nameof(FilteredProducts));
            OnPropertyChanged(nameof(FilteredProductsForSale));
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
