using Dapper;
using Microsoft.Data.Sqlite;
using RedDiceFlow.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedDiceFlow.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RedDiceFlow");

            Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "reddiceflow.db");
            _connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                ForeignKeys = true
            }.ToString();

            CreateTables();
            MigrateSalesIfNeeded();
        }

        private SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        private void CreateTables()
        {
            using var connection = CreateConnection();
            connection.Open();

            connection.Execute("PRAGMA foreign_keys = ON;");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Products
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Sku TEXT NOT NULL,
                    Price REAL NOT NULL,
                    Stock INTEGER NOT NULL,
                    Genre TEXT NOT NULL,
                    PlayersCount INTEGER NOT NULL DEFAULT 2
                );

                CREATE TABLE IF NOT EXISTS Customers
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName TEXT NOT NULL,
                    Phone TEXT NOT NULL UNIQUE,
                    Address TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Orders
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId INTEGER NULL,
                    CustomerPhone TEXT NOT NULL,
                    TotalPrice REAL NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY(CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL
                );

                CREATE TABLE IF NOT EXISTS OrderItems
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    ProductId INTEGER NULL,
                    ProductName TEXT NOT NULL,
                    Sku TEXT NOT NULL,
                    Genre TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    LineTotal REAL NOT NULL,
                    FOREIGN KEY(OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
                    FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE SET NULL
                );
            ");

            AddMissingProductColumns(connection);
            AddMissingOrderColumns(connection);
        }

        private static void AddMissingProductColumns(SqliteConnection connection)
        {
            var columns = connection
                .Query<string>("SELECT name FROM pragma_table_info('Products');")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!columns.Contains("Genre"))
                connection.Execute("ALTER TABLE Products ADD COLUMN Genre TEXT NOT NULL DEFAULT '';");

            if (!columns.Contains("PlayersCount"))
                connection.Execute("ALTER TABLE Products ADD COLUMN PlayersCount INTEGER NOT NULL DEFAULT 2;");
        }

        private static void AddMissingOrderColumns(SqliteConnection connection)
        {
            var columns = connection
                .Query<string>("SELECT name FROM pragma_table_info('Orders');")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!columns.Contains("CustomerId"))
                connection.Execute("ALTER TABLE Orders ADD COLUMN CustomerId INTEGER NULL;");
        }

        private void MigrateSalesIfNeeded()
        {
            using var connection = CreateConnection();
            connection.Open();

            var tableExists = connection.QuerySingleOrDefault<string>(@"
                SELECT name FROM sqlite_master
                WHERE type='table' AND name='Sales';
            ");

            if (tableExists == null)
                return;

            var count = connection.QuerySingle<int>("SELECT COUNT(*) FROM Sales;");

            if (count == 0)
            {
                connection.Execute("DROP TABLE IF EXISTS Sales;");
                return;
            }

            var sales = connection.Query(@"
                SELECT Id, ProductId, ProductName, Sku, Genre, Quantity, UnitPrice, TotalPrice, SoldAt
                FROM Sales
                ORDER BY SoldAt;
            ").AsList();

            using var transaction = connection.BeginTransaction();

            foreach (var sale in sales)
            {
                var orderId = connection.ExecuteScalar<int>(@"
                    INSERT INTO Orders (CustomerId, CustomerPhone, TotalPrice, CreatedAt)
                    VALUES (NULL, '-', @TotalPrice, @SoldAt);

                    SELECT last_insert_rowid();
                ", new { TotalPrice = (double)sale.TotalPrice, SoldAt = (string)sale.SoldAt }, transaction);

                connection.Execute(@"
                    INSERT INTO OrderItems
                    (OrderId, ProductId, ProductName, Sku, Genre, Quantity, UnitPrice, LineTotal)
                    VALUES
                    (@OrderId, @ProductId, @ProductName, @Sku, @Genre, @Quantity, @UnitPrice, @TotalPrice);
                ", new
                {
                    OrderId = orderId,
                    ProductId = (int?)sale.ProductId,
                    ProductName = (string)sale.ProductName,
                    Sku = (string)sale.Sku,
                    Genre = (string)sale.Genre,
                    Quantity = (int)sale.Quantity,
                    UnitPrice = (double)sale.UnitPrice,
                    TotalPrice = (double)sale.TotalPrice
                }, transaction);
            }

            connection.Execute("DROP TABLE IF EXISTS Sales;", transaction: transaction);

            transaction.Commit();
        }

        public IReadOnlyList<Customer> GetCustomers()
        {
            using var connection = CreateConnection();

            return connection.Query<Customer>(@"
                SELECT Id, FullName, Phone, Address, CreatedAt
                FROM Customers
                ORDER BY FullName;
            ").AsList();
        }

        public IReadOnlyList<Customer> SearchCustomers(string searchText)
        {
            using var connection = CreateConnection();

            return connection.Query<Customer>(@"
                SELECT Id, FullName, Phone, Address, CreatedAt
                FROM Customers
                WHERE FullName LIKE @Search
                   OR Phone LIKE @Search
                   OR Address LIKE @Search
                ORDER BY FullName;
            ", new { Search = $"%{searchText.Trim()}%" }).AsList();
        }

        public int AddCustomer(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FullName))
                throw new ArgumentException("Customer name is required.");

            if (string.IsNullOrWhiteSpace(customer.Phone))
                throw new ArgumentException("Customer phone is required.");

            using var connection = CreateConnection();

            return connection.ExecuteScalar<int>(@"
                INSERT INTO Customers (FullName, Phone, Address, CreatedAt)
                VALUES (@FullName, @Phone, @Address, @CreatedAt);

                SELECT last_insert_rowid();
            ", new
            {
                FullName = customer.FullName.Trim(),
                Phone = customer.Phone.Trim(),
                Address = customer.Address.Trim(),
                CreatedAt = DateTime.Now
            });
        }

        public Customer? GetCustomerByPhone(string phone)
        {
            using var connection = CreateConnection();

            return connection.QuerySingleOrDefault<Customer>(@"
                SELECT Id, FullName, Phone, Address, CreatedAt
                FROM Customers
                WHERE Phone = @Phone;
            ", new { Phone = phone.Trim() });
        }

        public IReadOnlyList<Customer> SearchCustomersFuzzy(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Customer>();

            using var connection = CreateConnection();

            return connection.Query<Customer>(@"
                SELECT Id, FullName, Phone, Address, CreatedAt
                FROM Customers
                WHERE FullName LIKE @Q
                   OR Phone LIKE @Q
                ORDER BY
                    CASE
                        WHEN Phone = @Exact THEN 0
                        WHEN FullName LIKE @Exact THEN 1
                        ELSE 2
                    END,
                    FullName;
            ", new { Q = $"%{query.Trim()}%", Exact = $"{query.Trim()}%" }).AsList();
        }

        public IReadOnlyList<Product> GetProducts()
        {
            using var connection = CreateConnection();

            return connection.Query<Product>(@"
                SELECT Id, Name, Sku, Price, Stock, Genre, PlayersCount
                FROM Products
                ORDER BY Name;
            ").AsList();
        }

        public int AddProduct(Product product)
        {
            using var connection = CreateConnection();

            return connection.ExecuteScalar<int>(@"
                INSERT INTO Products (Name, Sku, Price, Stock, Genre, PlayersCount)
                VALUES (@Name, @Sku, @Price, @Stock, @Genre, @PlayersCount);

                SELECT last_insert_rowid();
            ", product);
        }

        public void UpdateProduct(Product product)
        {
            if (product.Id == 0)
                return;

            using var connection = CreateConnection();

            connection.Execute(@"
                UPDATE Products
                SET Name = @Name,
                    Sku = @Sku,
                    Price = @Price,
                    Stock = @Stock,
                    Genre = @Genre,
                    PlayersCount = @PlayersCount
                WHERE Id = @Id;
            ", product);
        }

        public void DeleteProduct(int id)
        {
            using var connection = CreateConnection();
            connection.Open();

            var orderCount = connection.QuerySingle<int>(@"
                SELECT COUNT(*) FROM OrderItems
                WHERE ProductId = @Id;
            ", new { Id = id });

            if (orderCount > 0)
                throw new InvalidOperationException(
                    "Cannot delete product: it is referenced in existing orders.");

            connection.Execute("DELETE FROM Products WHERE Id = @Id;", new { Id = id });
        }

        public IReadOnlyList<Order> GetOrders()
        {
            using var connection = CreateConnection();

            return connection.Query<Order>(@"
                SELECT o.Id,
                       o.CustomerId,
                       COALESCE(c.FullName, '') AS CustomerName,
                       o.CustomerPhone,
                       o.TotalPrice,
                       o.CreatedAt,
                       COUNT(oi.Id) AS ItemsCount
                FROM Orders o
                LEFT JOIN OrderItems oi ON oi.OrderId = o.Id
                LEFT JOIN Customers c ON c.Id = o.CustomerId
                GROUP BY o.Id, o.CustomerId, c.FullName, o.CustomerPhone, o.TotalPrice, o.CreatedAt
                ORDER BY datetime(o.CreatedAt) DESC;
            ").AsList();
        }

        public IReadOnlyList<Order> SearchOrdersByCustomerPhone(string customerPhone)
        {
            using var connection = CreateConnection();

            return connection.Query<Order>(@"
                SELECT o.Id,
                       o.CustomerId,
                       COALESCE(c.FullName, '') AS CustomerName,
                       o.CustomerPhone,
                       o.TotalPrice,
                       o.CreatedAt,
                       COUNT(oi.Id) AS ItemsCount
                FROM Orders o
                LEFT JOIN OrderItems oi ON oi.OrderId = o.Id
                LEFT JOIN Customers c ON c.Id = o.CustomerId
                WHERE o.CustomerPhone LIKE @Phone
                   OR c.FullName LIKE @Phone
                GROUP BY o.Id, o.CustomerId, c.FullName, o.CustomerPhone, o.TotalPrice, o.CreatedAt
                ORDER BY datetime(o.CreatedAt) DESC;
            ", new { Phone = $"%{customerPhone.Trim()}%" }).AsList();
        }

        public IReadOnlyList<OrderItem> GetOrderItems(int orderId)
        {
            using var connection = CreateConnection();

            return connection.Query<OrderItem>(@"
                SELECT Id, OrderId, ProductId, ProductName, Sku, Genre, Quantity, UnitPrice, LineTotal
                FROM OrderItems
                WHERE OrderId = @OrderId
                ORDER BY Id;
            ", new { OrderId = orderId }).AsList();
        }

        public IReadOnlyList<Order> GetOrdersByCustomerId(int customerId)
        {
            using var connection = CreateConnection();

            return connection.Query<Order>(@"
                SELECT Id, CustomerId, CustomerPhone, TotalPrice, CreatedAt, 0 AS ItemsCount
                FROM Orders
                WHERE CustomerId = @Id
                ORDER BY datetime(CreatedAt) DESC;
            ", new { Id = customerId }).AsList();
        }

        public void CancelOrder(int orderId)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var items = connection.Query<OrderItem>(@"
                SELECT Id, OrderId, ProductId, ProductName, Sku, Genre, Quantity, UnitPrice, LineTotal
                FROM OrderItems
                WHERE OrderId = @OrderId;
            ", new { OrderId = orderId }, transaction).AsList();

            foreach (var item in items)
            {
                if (item.ProductId == null)
                    continue;

                connection.Execute(@"
                    UPDATE Products
                    SET Stock = Stock + @Quantity
                    WHERE Id = @ProductId;
                ", new
                {
                    ProductId = item.ProductId,
                    item.Quantity
                }, transaction);
            }

            connection.Execute("DELETE FROM Orders WHERE Id = @Id;",
                new { Id = orderId }, transaction);

            transaction.Commit();
        }

        public int AddOrder(string customerPhone, IReadOnlyList<OrderItem> items, string? customerName = null)
        {
            if (string.IsNullOrWhiteSpace(customerPhone))
                throw new ArgumentException("Customer phone is required.");

            if (items.Count == 0)
                throw new ArgumentException("Order must contain at least one product.");

            using var connection = CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var preparedItems = new List<OrderItem>();

            foreach (var item in items.Where(i => i.Quantity > 0))
            {
                if (item.ProductId is null or <= 0)
                    throw new ArgumentException("Order item must have a product.");

                var product = connection.QuerySingleOrDefault<Product>(@"
                    SELECT Id, Name, Sku, Price, Stock, Genre, PlayersCount
                    FROM Products
                    WHERE Id = @Id;
                ", new { Id = item.ProductId }, transaction);

                if (product == null)
                    throw new InvalidOperationException("Product was not found.");

                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"Not enough stock for {product.Name}.");

                preparedItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    Genre = product.Genre,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = product.Price * item.Quantity
                });
            }

            if (preparedItems.Count == 0)
                throw new ArgumentException("Order must contain at least one valid product.");

            var totalPrice = preparedItems.Sum(i => i.LineTotal);
            var customer = GetOrCreateCustomerForOrder(connection, transaction, customerPhone.Trim(), customerName);

            var orderId = connection.ExecuteScalar<int>(@"
                INSERT INTO Orders (CustomerId, CustomerPhone, TotalPrice, CreatedAt)
                VALUES (@CustomerId, @CustomerPhone, @TotalPrice, @CreatedAt);

                SELECT last_insert_rowid();
            ", new
            {
                CustomerId = customer.Id,
                CustomerPhone = customerPhone.Trim(),
                TotalPrice = totalPrice,
                CreatedAt = DateTime.Now
            }, transaction);

            foreach (var item in preparedItems)
            {
                var changedRows = connection.Execute(@"
                    UPDATE Products
                    SET Stock = Stock - @Quantity
                    WHERE Id = @ProductId AND Stock >= @Quantity;
                ", new
                {
                    ProductId = item.ProductId,
                    item.Quantity
                }, transaction);

                if (changedRows == 0)
                    throw new InvalidOperationException($"Not enough stock for {item.ProductName}.");

                connection.Execute(@"
                    INSERT INTO OrderItems
                    (OrderId, ProductId, ProductName, Sku, Genre, Quantity, UnitPrice, LineTotal)
                    VALUES
                    (@OrderId, @ProductId, @ProductName, @Sku, @Genre, @Quantity, @UnitPrice, @LineTotal);
                ", new
                {
                    OrderId = orderId,
                    item.ProductId,
                    item.ProductName,
                    item.Sku,
                    item.Genre,
                    item.Quantity,
                    item.UnitPrice,
                    item.LineTotal
                }, transaction);
            }

            transaction.Commit();
            return orderId;
        }

        private static Customer GetOrCreateCustomerForOrder(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string customerPhone,
            string? customerName = null)
        {
            var customer = connection.QuerySingleOrDefault<Customer>(@"
                SELECT Id, FullName, Phone, Address, CreatedAt
                FROM Customers
                WHERE Phone = @Phone;
            ", new { Phone = customerPhone }, transaction);

            if (customer != null)
            {
                if (!string.IsNullOrWhiteSpace(customerName) && customer.FullName != customerName.Trim())
                {
                    connection.Execute(@"
                        UPDATE Customers
                        SET FullName = @FullName
                        WHERE Id = @Id;
                    ", new { FullName = customerName.Trim(), customer.Id }, transaction);

                    customer.FullName = customerName.Trim();
                }
                return customer;
            }

            var nameToUse = string.IsNullOrWhiteSpace(customerName)
                ? $"Customer {customerPhone}"
                : customerName.Trim();

            var customerId = connection.ExecuteScalar<int>(@"
                INSERT INTO Customers (FullName, Phone, Address, CreatedAt)
                VALUES (@FullName, @Phone, @Address, @CreatedAt);

                SELECT last_insert_rowid();
            ", new
            {
                FullName = nameToUse,
                Phone = customerPhone,
                Address = string.Empty,
                CreatedAt = DateTime.Now
            }, transaction);

            return new Customer
            {
                Id = customerId,
                FullName = nameToUse,
                Phone = customerPhone,
                Address = string.Empty,
                CreatedAt = DateTime.Now
            };
        }
    }
}
