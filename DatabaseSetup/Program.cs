using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data.Common;

class Program
{
    private static string? _connectionString;
    private static string? _databaseName;
    public static void Main(string[] args)
    {
        LoadConfiguration();
        if (string.IsNullOrEmpty(_connectionString))
        {
            Console.WriteLine("No Connection String Found. Aborting!");
            return;
        }
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

        if (builder.TryGetValue("Database", out var databaseName) && databaseName != null)
        {
            return databaseName.ToString();
        }

        throw new InvalidOperationException("Database name not found in the connection string.");
    }

    private static void InitializeDatabase()
    {
        using var connection = new SqlConnection(_connectionString);
        
        // Create database if it doesn't exist
        var masterConnection = new SqlConnection(GetMasterConnectionString());
        masterConnection.Open();
        
        if (!DatabaseExists(masterConnection))
        {
            CreateDatabase(masterConnection);
            Console.WriteLine($"Database {_databaseName} created.");
            
            // Apply schema
            connection.Open();
            string schemaPath = GetFile();
            ExecuteSqlFile(connection, schemaPath);
            Console.WriteLine("Schema applied successfully.");
            
            // Apply seed data
            string seedPath = GetSeedFile();
            ExecuteSqlFile(connection, seedPath);
            Console.WriteLine("Seed data inserted successfully.");
        }
        else
        {
            Console.WriteLine($"Database {_databaseName} already exists.");
        }
    }

    private static void ResetDatabase()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            
            // Create database if it doesn't exist
            var masterConnection = new SqlConnection(GetMasterConnectionString());
            masterConnection.Open();
            
            // Drop and recreate database
            CreateDatabase(masterConnection);
            Console.WriteLine($"Database {_databaseName} dropped and recreated.");
            
            // Apply schema
            connection.Open();
            string schemaPath = GetFile();
            ExecuteSqlFile(connection, schemaPath);
            Console.WriteLine("Schema applied successfully.");
            
            // Apply seed data
            string seedPath = GetSeedFile();
            ExecuteSqlFile(connection, seedPath);
            Console.WriteLine("Seed data inserted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static bool DatabaseExists(SqlConnection connection)
    {
        using var command = new SqlCommand($"SELECT database_id FROM sys.databases WHERE name = '{_databaseName}'", connection);
        return command.ExecuteScalar() != null;
    }

    private static void ExecuteSql(SqlConnection connection, string sql)
    {
        using var command = new SqlCommand(sql, connection);
        
        // Only add parameters for non-DDL statements
        if (sql.Contains("@dbName"))
        {
            command.Parameters.AddWithValue("@dbName", _databaseName);
        }
        
        command.ExecuteNonQuery();
    }

    private static void ExecuteSqlFile(SqlConnection connection, string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"SQL file not found: {filePath}");
        }

        string script = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(script))
        {
            throw new InvalidOperationException($"SQL file is empty: {filePath}");
        }

        ExecuteSql(connection, script);
    }

    private static string GetProjectRoot()
    {
        string currentDir = AppContext.BaseDirectory;
        DirectoryInfo? dir = new DirectoryInfo(currentDir);
        
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Informatics.Appetite.sln")))
        {
            dir = dir.Parent;
        }
        
        if (dir == null)
        {
            throw new DirectoryNotFoundException("Could not find solution root directory");
        }
        
        return dir.FullName;
    }

    private static string GetFile()
    {
        string projectRoot = GetProjectRoot();
        string schemaFilePath = Path.Combine(projectRoot, "DatabaseSetup", "Database", "schema.sql");
        
        if (!File.Exists(schemaFilePath))
        {
            throw new FileNotFoundException($"Schema file not found at: {schemaFilePath}");
        }
        
        return schemaFilePath;
    }

    private static string GetSeedFile()
    {
        string projectRoot = GetProjectRoot();
        string seedFilePath = Path.Combine(projectRoot, "DatabaseSetup", "Database", "seed.sql");
        
        if (!File.Exists(seedFilePath))
        {
            throw new FileNotFoundException($"Seed file not found at: {seedFilePath}");
        }
        
        return seedFilePath;
    }

    private static string GetMasterConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"  // Connect to master database
        };
        return builder.ConnectionString;
    }

    private static void CreateDatabase(SqlConnection masterConnection)
    {
        // Validate database name for safety
        if (string.IsNullOrEmpty(_databaseName) || _databaseName.Contains(";"))
        {
            throw new InvalidOperationException("Invalid database name");
        }

        // Drop database if it exists (safety check)
        var dropSql = $@"
            IF EXISTS (SELECT * FROM sys.databases WHERE name = '{_databaseName}')
            BEGIN
                ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_databaseName}];
            END";

        // Create new database
        var createSql = $"CREATE DATABASE [{_databaseName}];";

        // Execute without parameters since DDL doesn't support them
        using (var command = new SqlCommand(dropSql, masterConnection))
        {
            command.ExecuteNonQuery();
        }

        using (var command = new SqlCommand(createSql, masterConnection))
        {
            command.ExecuteNonQuery();
        }
    }
}
