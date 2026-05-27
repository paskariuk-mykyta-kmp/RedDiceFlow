using Dapper;
using Microsoft.Data.Sqlite;
using RedDiceFlow.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedDiceFlow.Services;

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

    private SqliteConnection CreateConnection() => new(_connectionString);

    private void CreateTables()
    {
        using var connection = CreateConnection();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS Products
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Sku TEXT NOT NULL,
                Price REAL NOT NULL,
                Stock INTEGER NOT NULL,
                Genre TEXT NOT NULL
            );
            """);
    }

    public IReadOnlyList<Product> GetProducts()
    {
        using var connection = CreateConnection();

        return connection.Query<Product>("""
            SELECT Id, Name, Sku, Price, Stock, Genre
            FROM Products
            ORDER BY Name;
            """).AsList();
    }

    public int AddProduct(Product product)
    {
        using var connection = CreateConnection();

        return connection.ExecuteScalar<int>("""
            INSERT INTO Products (Name, Sku, Price, Stock, Genre)
            VALUES (@Name, @Sku, @Price, @Stock, @Genre)
            RETURNING Id;
            """, product);
    }

    public void UpdateProduct(Product product)
    {
        using var connection = CreateConnection();

        connection.Execute("""
            UPDATE Products
            SET Name = @Name,
                Sku = @Sku,
                Price = @Price,
                Stock = @Stock,
                Genre = @Genre
            WHERE Id = @Id;
            """, product);
    }

    public void DeleteProduct(int id)
    {
        using var connection = CreateConnection();

        connection.Execute("""
            DELETE FROM Products
            WHERE Id = @Id;
            """, new { Id = id });
    }
}