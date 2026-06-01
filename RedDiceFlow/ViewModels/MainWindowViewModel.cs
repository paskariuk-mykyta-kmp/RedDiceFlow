using CommunityToolkit.Mvvm.Input;
using RedDiceFlow.Models;
using RedDiceFlow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RedDiceFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService = new();
        private readonly ReportService _reportService = new();
        private bool _isUkrainian = false;
        private static readonly string _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RedDiceFlow", "config.json");

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
                SaveConfig();
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
        public ObservableCollection<string> GenreOptions { get; } = new();
        public string[] PlayersCountOptions { get; } = new[] { "1", "2", "3", "4", "5", "6", "7", "8" };

        private bool _isEditMode;
        public bool IsEditMode { get => _isEditMode; set { _isEditMode = value; OnPropertyChanged(); } }

        private Product? _editTargetProduct;
        public Product? EditTargetProduct { get => _editTargetProduct; set { _editTargetProduct = value; OnPropertyChanged(); } }

        private bool _isInStoreSale = true;
        public bool IsInStoreSale { get => _isInStoreSale; set { _isInStoreSale = value; OnPropertyChanged(); } }

        private string _sortMode = "Name";
        public string SortMode
        {
            get => _sortMode;
            set { _sortMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredProducts)); }
        }

        public event Func<Product?, Task<bool>>? ConfirmDeleteRequested;
        public event Func<Order?, Task<bool>>? ConfirmCancelOrderRequested;
        public event Func<Order?, Task<bool>>? ConfirmDeleteOrderRequested;

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Order> Orders { get; set; }
        public ObservableCollection<OrderItem> SelectedOrderItems { get; set; } = new();
        public ObservableCollection<OrderItem> CartItems { get; set; } = new();
        private ObservableCollection<Customer> _foundCustomers = new();
        public ObservableCollection<Customer> FoundCustomers
        {
            get => _foundCustomers;
            set { _foundCustomers = value; OnPropertyChanged(); }
        }

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

        private string _quickSearchText = string.Empty;
        public string QuickSearchText
        {
            get => _quickSearchText;
            set
            {
                _quickSearchText = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Order> _quickSearchResults = new();
        public ObservableCollection<Order> QuickSearchResults
        {
            get => _quickSearchResults;
            set { _quickSearchResults = value; OnPropertyChanged(); }
        }

        public bool HasQuickSearchResult => QuickSearchResults.Count > 0;

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
            set
            {
                _selectedProductForCart = value;
                OnPropertyChanged();
                if (value != null)
                    CartQuantity = "1";
            }
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
        public int SoldItems => Orders.Where(o => o.Status == "delivered").Sum(o => o.ItemsCount);
        public double SalesTotal => Orders.Where(o => o.Status == "delivered").Sum(o => o.TotalPrice);
        public double SalesToday => Orders.Where(o => o.Status == "delivered" && o.CreatedAt.Date == DateTime.Today).Sum(o => o.TotalPrice);
        public double DashboardSalesToday => Orders.Where(o => o.Status != "cancelled" && o.CreatedAt.Date == DateTime.Today).Sum(o => o.TotalPrice);
        public double SalesThisWeek => Orders.Where(o => o.Status == "delivered" && o.CreatedAt >= DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek)).Sum(o => o.TotalPrice);
        public double SalesThisMonth => Orders.Where(o => o.Status == "delivered" && o.CreatedAt.Year == DateTime.Today.Year && o.CreatedAt.Month == DateTime.Today.Month).Sum(o => o.TotalPrice);
        public bool HasNoRecentOrders => Orders.Count == 0;
        public IEnumerable<Order> RecentOrders => Orders.Take(3);

        private string _orderStatusFilter = "All";
        public string OrderStatusFilter
        {
            get => _orderStatusFilter;
            set
            {
                _orderStatusFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredOrders));
            }
        }

        public IEnumerable<Order> FilteredOrders
        {
            get
            {
                if (OrderStatusFilter == "All")
                    return Orders;
                return Orders.Where(o => o.Status == OrderStatusFilter);
            }
        }

        public List<string> OrderStatusFilterOptions { get; } = new() { "All", "sold", "shipped", "delivered", "cancelled" };
        public IEnumerable<Product> LowStockProducts => Products.Where(p => p.Stock <= 5);
        public ObservableCollection<TopSellingGame> TopSellingGames { get; set; } = new();

        public double StockLoadPercent => Math.Min(100, TotalStock / Math.Max(1, StockLoadGoal) * 100);
        public double SalesProgressPercent => Math.Min(100, SalesTotal / Math.Max(1, SalesProgressGoal) * 100);
        public double LowStockRiskPercent => TotalProducts == 0 ? 0 : Math.Round((double)LowStockCount / TotalProducts * 100);

        private double _stockLoadGoal = 100;
        public double StockLoadGoal
        {
            get => _stockLoadGoal;
            set { _stockLoadGoal = value; OnPropertyChanged(); OnPropertyChanged(nameof(StockLoadPercent)); SaveConfig(); }
        }

        private double _salesProgressGoal = 1000;
        public double SalesProgressGoal
        {
            get => _salesProgressGoal;
            set { _salesProgressGoal = value; OnPropertyChanged(); OnPropertyChanged(nameof(SalesProgressPercent)); SaveConfig(); }
        }

        private string _newGenreText = string.Empty;
        public string NewGenreText
        {
            get => _newGenreText;
            set { _newGenreText = value; OnPropertyChanged(); }
        }

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
        public ICommand AddSelectedToCartCommand { get; }
        public ICommand ShipOrderCommand { get; }
        public ICommand DeliverOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand QuickSearchOrderCommand { get; }
        public ICommand AddGenreCommand { get; }

        public MainWindowViewModel()
        {
            Products = new ObservableCollection<Product>(_databaseService.GetProducts());

            var genres = _databaseService.GetGenres();
            foreach (var g in genres)
                GenreOptions.Add(g);

            Orders = new ObservableCollection<Order>(_databaseService.GetOrders());
            UpdateDisplayNumbers();

            foreach (var product in Products)
                product.PropertyChanged += OnProductChanged;

            RefreshTopSellingGames();

            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            RemoveFromCartCommand = new RelayCommand<OrderItem>(RemoveFromCart);
            PlaceOrderCommand = new RelayCommand(PlaceOrder);
            CancelOrderCommand = new RelayCommand<Order>(CancelOrder);
            SearchOrdersCommand = new RelayCommand(SearchOrders);
            ReloadOrdersCommand = new RelayCommand(ReloadOrders);
            QuickSearchOrderCommand = new RelayCommand(QuickSearchOrder);
            EditProductCommand = new RelayCommand<Product>(EditProduct);
            RestockProductCommand = new RelayCommand<Product>(RestockProduct);
            DeleteProductCommand = new RelayCommand<Product>(DeleteProduct);
            CancelEditCommand = new RelayCommand(CancelEdit);
            AddSelectedToCartCommand = new RelayCommand(AddSelectedToCart);
            ShipOrderCommand = new RelayCommand<Order>(ShipOrder);
            DeliverOrderCommand = new RelayCommand<Order>(DeliverOrder);
            DeleteOrderCommand = new RelayCommand<Order>(DeleteOrder);
            AddGenreCommand = new RelayCommand(AddGenre);

            LoadConfig();
            UpdatePeriodLabels();
        }

        public void SortBy(string mode)
        {
            SortMode = mode;
        }

        public void ToggleLanguage()
        {
            _isUkrainian = !_isUkrainian;
            App.IsUkrainianStatic = _isUkrainian;
            App.SwitchLanguage(_isUkrainian);
            UpdatePeriodLabels();
            OnPropertyChanged((string?)null);
            SaveConfig();
        }

        private void UpdatePeriodLabels()
        {
            if (_isUkrainian)
            {
                ReportPeriods[0] = "Сьогодні";
                ReportPeriods[1] = "Тиждень";
                ReportPeriods[2] = "Місяць";
                ReportPeriods[3] = "За весь час";
            }
            else
            {
                ReportPeriods[0] = "Today";
                ReportPeriods[1] = "This Week";
                ReportPeriods[2] = "This Month";
                ReportPeriods[3] = "All Time";
            }
        }

        private void LoadConfig()
        {
            if (!File.Exists(_configPath)) return;

            try
            {
                var json = File.ReadAllText(_configPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data == null) return;

                if (data.TryGetValue("IsUkrainian", out var ua) && ua.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    _isUkrainian = true;
                    App.IsUkrainianStatic = true;
                    App.SwitchLanguage(true);
                }
                if (data.TryGetValue("IsLightTheme", out var lt) && lt.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    _isLightTheme = true;
                    OnPropertyChanged(nameof(IsLightTheme));
                    App.SwitchTheme(true);
                }
                if (data.TryGetValue("StockLoadGoal", out var slg) && double.TryParse(slg, out var loadGoal))
                    StockLoadGoal = loadGoal;
                if (data.TryGetValue("SalesProgressGoal", out var spg) && double.TryParse(spg, out var salesGoal))
                    SalesProgressGoal = salesGoal;
            }
            catch { }
        }

        private void SaveConfig()
        {
            try
            {
                var dir = Path.GetDirectoryName(_configPath);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var data = new Dictionary<string, string>
                {
                    ["IsUkrainian"] = _isUkrainian.ToString().ToLowerInvariant(),
                    ["IsLightTheme"] = _isLightTheme.ToString().ToLowerInvariant(),
                    ["StockLoadGoal"] = _stockLoadGoal.ToString(CultureInfo.InvariantCulture),
                    ["SalesProgressGoal"] = _salesProgressGoal.ToString(CultureInfo.InvariantCulture),
                };
                File.WriteAllText(_configPath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
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

            if (string.IsNullOrWhiteSpace(NewSku))
                NewSku = GenerateSku();

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

        public void AddSelectedToCart()
        {
            AddToCart(SelectedProductForCart!);
            if (SelectedProductForCart != null)
            {
                SaleSearchText = string.Empty;
                SelectedProductForCart = null;
            }
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
            if (string.IsNullOrWhiteSpace(CustomerSearchText))
            {
                FoundCustomers = new ObservableCollection<Customer>();
                SelectedFoundCustomer = null;
                NewCustomerName = string.Empty;
                OnPropertyChanged(nameof(HasFoundCustomer));
                return;
            }

            var results = _databaseService.SearchCustomersFuzzy(CustomerSearchText);
            FoundCustomers = new ObservableCollection<Customer>(results);

            if (FoundCustomers.Count == 0)
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

                var orderId = _databaseService.AddOrder(phone, items, name);

                if (IsInStoreSale)
                    _databaseService.UpdateOrderStatus(orderId, "delivered");

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
                StatusMessage = _isUkrainian ? "Замовлення оформлено" : "Order placed successfully";
                RefreshAnalytics();
                RefreshProducts();
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
                RefreshProducts();
                StatusMessage = _isUkrainian ? "Замовлення скасовано, залишки повернено" : "Order cancelled, stock restored";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public void ShipOrder(Order? order)
        {
            if (order == null || order.Status != "sold")
                return;

            _databaseService.UpdateOrderStatus(order.Id, "shipped");
            order.Status = "shipped";
            RefreshRecentOrders();
            StatusMessage = _isUkrainian ? "Замовлення відправлено" : "Order shipped";
        }

        public void DeliverOrder(Order? order)
        {
            if (order == null || order.Status != "shipped")
                return;

            _databaseService.UpdateOrderStatus(order.Id, "delivered");
            order.Status = "delivered";
            RefreshRecentOrders();
            StatusMessage = _isUkrainian ? "Замовлення доставлено" : "Order delivered";
        }

        public async void DeleteOrder(Order? order)
        {
            if (order == null)
                return;

            if (ConfirmDeleteOrderRequested != null)
            {
                var confirmed = await ConfirmDeleteOrderRequested(order);
                if (!confirmed)
                    return;
            }

            try
            {
                _databaseService.HardDeleteOrder(order.Id);
                RefreshRecentOrders();
                RefreshAnalytics();
                RefreshProducts();
                StatusMessage = _isUkrainian ? "Замовлення видалено" : "Order deleted";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public void AddGenre()
        {
            var name = NewGenreText?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            if (GenreOptions.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                StatusMessage = _isUkrainian ? "Жанр вже існує" : "Genre already exists";
                return;
            }

            _databaseService.AddGenre(name);
            GenreOptions.Add(name);
            NewGenreText = string.Empty;
            StatusMessage = _isUkrainian ? "Жанр додано" : "Genre added";
        }

        public void RefreshRecentOrders()
        {
            Orders.Clear();
            foreach (var order in _databaseService.GetOrders())
                Orders.Add(order);

            UpdateDisplayNumbers();
            OnPropertyChanged(nameof(RecentOrders));
            OnPropertyChanged(nameof(HasNoRecentOrders));
            OnPropertyChanged(nameof(FilteredOrders));
        }

        private void RefreshProducts()
        {
            foreach (var product in Products)
                product.PropertyChanged -= OnProductChanged;

            Products.Clear();
            foreach (var product in _databaseService.GetProducts())
            {
                product.PropertyChanged += OnProductChanged;
                Products.Add(product);
            }
        }

        private void UpdateDisplayNumbers()
        {
            for (int i = 0; i < Orders.Count; i++)
                Orders[i].DisplayNumber = i + 1;
        }

        public ObservableCollection<string> ReportPeriods { get; } = new() { "Today", "This Week", "This Month", "All Time" };

        private int _selectedReportPeriodIndex = 3;
        public int SelectedReportPeriodIndex
        {
            get => _selectedReportPeriodIndex;
            set { _selectedReportPeriodIndex = value; OnPropertyChanged(); }
        }

        public void CreateManagerReport()
        {
            var periodKey = SelectedReportPeriodIndex switch
            {
                0 => "today",
                1 => "week",
                2 => "month",
                _ => "all"
            };
            var path = _reportService.CreateManagerReport(Products, Orders, periodKey);
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
            OnPropertyChanged(nameof(FilteredOrders));
        }

        public void ReloadOrders()
        {
            OrderSearchPhone = string.Empty;
            SearchOrders();
        }

        public void QuickSearchOrder()
        {
            QuickSearchResults.Clear();

            if (string.IsNullOrWhiteSpace(QuickSearchText))
                return;

            var query = QuickSearchText.Trim();
            var found = Orders.Where(o =>
                o.Id.ToString() == query ||
                o.OrderNumber.Contains(query, StringComparison.OrdinalIgnoreCase));

            foreach (var order in found)
                QuickSearchResults.Add(order);

            OnPropertyChanged(nameof(HasQuickSearchResult));
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
            OnPropertyChanged((string?)null);
            RefreshTopSellingGames();
        }

        private void RefreshTopSellingGames()
        {
            TopSellingGames.Clear();
            var deliveredOrderIds = Orders.Where(o => o.Status == "delivered").Select(o => o.Id).ToHashSet();
            var allItems = _databaseService.GetAllOrderItems().Where(i => deliveredOrderIds.Contains(i.OrderId));
            var top = allItems
                .GroupBy(i => new { i.ProductId, i.ProductName, i.Genre })
                .Select(g => new TopSellingGame
                {
                    Name = g.Key.ProductName,
                    Genre = g.Key.Genre ?? "",
                    TotalSold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.LineTotal)
                })
                .OrderByDescending(g => g.TotalSold)
                .Take(5);

            foreach (var game in top)
                TopSellingGames.Add(game);
        }

        private void OnProductChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Product product)
            {
                _databaseService.UpdateProduct(product);
                RefreshAnalytics();
            }
        }

        private string GenerateSku()
        {
            var random = Random.Shared.Next(10000, 99999);
            return $"RD-{random}";
        }
    }
}
