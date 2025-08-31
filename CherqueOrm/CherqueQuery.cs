using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace CherqueOrm;

public sealed class CherqueQuery(ILogger<CherqueQuery> logger, IConfiguration config)
{
    public async Task<List<T>> ExecuteQueryAsync<T>(string sql, SqlParameter[]? parameters,
        string? conStrOverride = null) where T : new()
    {
        List<T> results = [];

        var conString = conStrOverride ?? config.GetSection("ConnectionString").Value;
        
        await using SqlConnection con = new(conString);
        await con.OpenAsync();
        await using SqlCommand? cmd = con.CreateCommand();
        cmd.CommandText = sql;
        if (parameters is { Length: > 0 })
        {
            cmd.Parameters.AddRange(parameters);
        }
        
        Type typeOfT = typeof(T);
        if (CherqueUtil.IsPrimitiveOrSimpleType(typeOfT))
        {
            
        }
        PropertyInfo[] properties = typeOfT.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Dictionary<string, PropertyInfo> propertyMap = new(StringComparer.OrdinalIgnoreCase);
        foreach (PropertyInfo prop in properties)
        {
            propertyMap[prop.Name] = prop;
        }

        await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            T item = MapToClassProperties<T>(reader, propertyMap);
            results.Add(item);
        }

        cmd.Parameters.Clear();
        return results;
    }

    private static T MapToClassProperties<T>(SqlDataReader reader, Dictionary<string, PropertyInfo> propertyMap) where T : new()
    {
        T item = new T();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
                
            if (!propertyMap.TryGetValue(columnName, out PropertyInfo? property)) continue;
            if (reader.IsDBNull(i))
            {
                property.SetValue(item, null);
            }
            else
            {
                object dbValue = reader.GetValue(i);
                    
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
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error setting property '{property.Name}' from column '{columnName}'. Error: {ex.Message}");
                    throw;
                }
            }
        }
        
        return item;
    }
}