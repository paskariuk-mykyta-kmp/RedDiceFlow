using Dapper;
using Microsoft.Data.Sqlite;
using RedDiceFlow.Models;
using System;
using System.Collections.Generic;
using System.IO;

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
                DataSource = dbPath
            }.ToString();

            CreateTables();
        }

        private SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        private void CreateTables()
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Products
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Sku TEXT NOT NULL,
                    Price REAL NOT NULL,
                    Stock INTEGER NOT NULL,
                    Genre TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Sales
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductId INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    Sku TEXT NOT NULL,
                    Genre TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    TotalPrice REAL NOT NULL,
                    SoldAt TEXT NOT NULL
                );
            ");
        }

        public IReadOnlyList<Product> GetProducts()
        {
            using var connection = CreateConnection();

            return connection.Query<Product>(@"
                SELECT Id, Name, Sku, Price, Stock, Genre
                FROM Products
                ORDER BY Name;
            ").AsList();
        }

        public int AddProduct(Product product)
        {
            using var connection = CreateConnection();

            return connection.ExecuteScalar<int>(@"
                INSERT INTO Products (Name, Sku, Price, Stock, Genre)
                VALUES (@Name, @Sku, @Price, @Stock, @Genre);

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
                    Genre = @Genre
                WHERE Id = @Id;
            ", product);
        }

        public void DeleteProduct(int id)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                DELETE FROM Products
                WHERE Id = @Id;
            ", new { Id = id });
        }

        public IReadOnlyList<Sale> GetSales()
        {
            using var connection = CreateConnection();

            return connection.Query<Sale>(@"
                SELECT Id, ProductId, ProductName, Sku, Genre, Quantity, UnitPrice, TotalPrice, SoldAt
                FROM Sales
                ORDER BY SoldAt DESC;
            ").AsList();
        }

        public void AddSale(Product product, int quantity)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var changedRows = connection.Execute(@"
                UPDATE Products
                SET Stock = Stock - @Quantity
                WHERE Id = @ProductId AND Stock >= @Quantity;
            ", new
            {
                ProductId = product.Id,
                Quantity = quantity
            }, transaction);

            if (changedRows == 0)
                throw new InvalidOperationException("Not enough stock.");

            connection.Execute(@"
                INSERT INTO Sales (ProductId, ProductName, Sku, Genre, Quantity, UnitPrice, TotalPrice, SoldAt)
                VALUES (@ProductId, @ProductName, @Sku, @Genre, @Quantity, @UnitPrice, @TotalPrice, @SoldAt);
            ", new
            {
                ProductId = product.Id,
                ProductName = product.Name,
                product.Sku,
                product.Genre,
                Quantity = quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * quantity,
                SoldAt = DateTime.Now
            }, transaction);

            transaction.Commit();
        }
    }
}