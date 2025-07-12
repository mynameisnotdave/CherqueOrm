using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;


namespace CherqueOrm;

public sealed class CherqueQuery
{
    public async Task<List<T>> ExecuteQueryAsync<T>(string sql, SqlParameter[]? parameters,
        string? conStrOverride = null) where T : new()
    {
        List<T> results = [];

        await using SqlConnection con = new(conStrOverride);
        await con.OpenAsync();
        await using SqlCommand? cmd = con.CreateCommand();
        cmd.CommandText = sql;
        if (parameters is { Length: > 0 })
        {
            cmd.Parameters.AddRange(parameters);
        }

        // Get the type of T and its public properties
        Type typeOfT = typeof(T);
        PropertyInfo[] properties = typeOfT.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Dictionary<string, PropertyInfo> propertyMap = new(StringComparer.OrdinalIgnoreCase);
        foreach (PropertyInfo prop in properties)
        {
            propertyMap[prop.Name] = prop;
        }

        await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            T item = new T(); // Create a new instance of T for each row

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);

                // Try to find a matching property in T
                if (!propertyMap.TryGetValue(columnName, out PropertyInfo? property)) continue;
                // Check if the database value is DBNull
                if (reader.IsDBNull(i))
                {
                    // If DBNull, set the property to its default value (null for reference types, 0 for int?, etc.)
                    property.SetValue(item, null);
                }
                else
                {
                    // Get the value from the reader
                    object dbValue = reader.GetValue(i);

                    // Handle type conversion (basic types)
                    // This is a simplified conversion. A robust ORM would have more sophisticated type handling.
                    Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    try
                    {
                        object convertedValue = Convert.ChangeType(dbValue, propertyType);
                        property.SetValue(item, convertedValue);
                    }
                    catch (InvalidCastException ex)
                    {
                        Console.WriteLine(
                            $"Warning: Could not convert column '{columnName}' (DB type: {dbValue.GetType().Name}) to property '{property.Name}' (C# type: {propertyType.Name}). Error: {ex.Message}");
                        // You might choose to throw an exception, log, or skip setting the property
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Error setting property '{property.Name}' from column '{columnName}'. Error: {ex.Message}");
                        throw; // Re-throw for unhandled exceptions
                    }
                }
                // else: Column in SQL result does not have a matching property in T. Ignore or log.
            }

            results.Add(item);
        }

        cmd.Parameters.Clear();
        return results;
    }
}