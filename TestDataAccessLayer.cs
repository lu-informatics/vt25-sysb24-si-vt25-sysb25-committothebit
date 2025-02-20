using System;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Appetite;

public class TestDataAccessLayer
{
    // Retrieves the connection string from the appsettings.json file and returns a SqlConnection object
    // Requires appsettings.json file with a connection string named "AppetiteDatabase"
    public SqlConnection GetDataBaseConnection()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            string appsettingsFileName = "Informatics.Appetite.appsettings.json";

            IConfigurationRoot configuration;
            
            using (var stream = assembly.GetManifestResourceStream(appsettingsFileName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{appsettingsFileName}' not found.");
                }

                configuration = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
            }

            string? connectionString = configuration.GetConnectionString("AppetiteDatabase");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'AppetiteDatabase' not found.");
            }

            SqlConnectionStringBuilder builder = new(connectionString);
            SqlConnection connection = new(builder.ConnectionString);

            return connection;
        }
        catch (SqlException ex)
        {
            Console.WriteLine("An error occurred while connecting to the database: " + ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An unexpected error occurred: " + ex.Message);
            throw;
        }
    }

    // Fetches all ingredients from the database and returns them as a list of strings containing id, name, and category, e.g. "1. Apple (Fruit)"
    public List<string> GetAllIngredients()
    {
        List<string> ingredients = new List<string>();

        using (SqlConnection connection = GetDataBaseConnection())
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Ingredient";

            connection.Open();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ingredients.Add(reader.GetInt32(0) + ". " + reader.GetString(1) + " (" + reader.GetString(2)+ ")");
                }
            }
        }

        return ingredients;
    }
}
