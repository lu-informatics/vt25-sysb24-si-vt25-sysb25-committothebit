using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data.Common;

class Program
{
    private static string? _connectionString;
    private static string _databaseName;
    public static void Main(string[] args)
    {
        LoadConfiguration();
        _databaseName = GetDatabaseName(_connectionString);
        Console.WriteLine("Choose an option:");
        Console.WriteLine("1 - Initialize Database (Only creates if missing)");
        Console.WriteLine("2 - Reset Database (Drops and recreates)");
        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                InitializeDatabase();
                Console.WriteLine("Database initialized successfully.");
                break;
            case "2":
                ResetDatabase();
                Console.WriteLine("Database reset successfully.");
                break;
            default:
                Console.WriteLine("Invalid choice. Exiting...");
                break;
        }
    }

    private static void LoadConfiguration()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = config.GetConnectionString("AppetiteDatabase");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Database connection string is missing in appsettings.json.");
        }
    }

    private static string GetDatabaseName(string connectionString)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        if (builder.TryGetValue("Database", out var databaseName))
        {
            return databaseName.ToString();
        }

        throw new InvalidOperationException("Database name not found in the connection string.");
    }

    private static void InitializeDatabase()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        if (!DatabaseExists(connection))
        {
            Console.WriteLine("Database does not exist. Creating...");
            ExecuteSql(connection, $"CREATE DATABASE {_databaseName};");
        }
        else
        {
            Console.WriteLine($"Database: {_databaseName}, already exists.");
        }
        
        connection.ChangeDatabase(_databaseName);
        /*
        if (!TableExists(connection, "Recipe"))
        {
            Console.WriteLine("Creating schema...");
            ExecuteSqlFile(connection, "Database/schema.sql");
        }
        else
        {
            Console.WriteLine("Schema already exists.");
        }

        if (!HasSeedData(connection))
        {
            Console.WriteLine("Seeding initial data...");
            ExecuteSqlFile(connection, "Database/init_data.sql");
        }
        else
        {
            Console.WriteLine("Data already exists.");
        }
        */
    }

    private static void ResetDatabase()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        connection.ChangeDatabase(_databaseName);

        Console.WriteLine("Dropping existing tables...");
        ExecuteSqlFile(connection, "Database/drop_tables.sql");

        Console.WriteLine("Recreating schema...");
        ExecuteSqlFile(connection, "Database/schema.sql");

        Console.WriteLine("Seeding initial data...");
        ExecuteSqlFile(connection, "Database/init_data.sql");
    }

    private static bool DatabaseExists(SqlConnection connection)
    {
        using var command = new SqlCommand($"SELECT database_id FROM sys.databases WHERE name = '{_databaseName}'", connection);
        return command.ExecuteScalar() != null;
    }

    private static bool TableExists(SqlConnection connection, string tableName)
    {
        using var command = new SqlCommand($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'", connection);
        return (int)command.ExecuteScalar() > 0;
    }

    private static bool HasSeedData(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT COUNT(*) FROM AppUser", connection);
        return (int)command.ExecuteScalar() > 0;
    }

    private static void ExecuteSql(SqlConnection connection, string sql)
    {
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private static void ExecuteSqlFile(SqlConnection connection, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"SQL file not found: {filePath}");
            return;
        }

        string script = File.ReadAllText(filePath);
        ExecuteSql(connection, script);
    }
}
