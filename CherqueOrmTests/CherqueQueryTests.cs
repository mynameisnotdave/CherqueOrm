using System.Data;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using CherqueOrm;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CherqueOrmTests;

public class CherqueQueryTests
{
    private const string connectionString = "Server=localhost,1433;Database=GroceriesDB;User Id=sa;Password=4#%fuPb5q6*sVe;TrustServerCertificate=True;";
    private readonly Mock<ILogger<CherqueQuery>> mockLogger = new();
    private const string connectionStringName = "DefaultConnection";
    private readonly Mock<IConfigurationSection> configSectionMock = new();
    private readonly Mock<IConfiguration> configMock = new();
    private CherqueQuery cherqueQuery;
    

    [SetUp]
    public void Setup()
    {
        configSectionMock.Setup(x => x.Value).Returns(connectionString);
        configMock.Setup(x => x.GetSection(connectionStringName)).Returns(configSectionMock.Object);
        configMock.SetupGet(c => c[$"ConnectionString:{connectionStringName}"]).Returns(connectionString);
        cherqueQuery = new CherqueQuery(mockLogger.Object, configMock.Object);
    }

    private readonly SqlParameter[] parameters = [
        new() { ParameterName = "ProductId", SqlDbType = SqlDbType.Int, Value = 1 }
    ];
    
    [Test]
    public void SqlParameterCollection_ShouldContainParametersAdded()
    {
        const string sql = "SELECT * FROM FoodProducts FP WHERE FP.ProductId = @ProductId";
        using SqlConnection con = new(connectionString);
        using SqlCommand cmd = new(sql, con);
        cmd.Parameters.AddRange(parameters);
        cmd.Parameters.Should().BeEquivalentTo(parameters);
    }

    [Test]
    public void SqlCommand_ShouldHaveInsertedSqlStatement()
    {
        const string sql = "SELECT FP.ProductName FROM FoodProducts FP";
        using SqlConnection con = new(connectionString);
        using SqlCommand cmd = new(sql, con);
        cmd.CommandText.Should().Be(sql);
    }

    // We're trying to avoid this and instead get it straight out of SqlDataReader without there being a need to explicitly define FOR JSON PATH in every query.
    [Test]
    public async Task QueryResultShouldCorrectlyDeserialize_BasicEdition()
    {
        const string sql = "SELECT * FROM FoodProducts FP WHERE FP.ProductId = @ProductId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER";
        await using SqlConnection con = new(connectionString);
        await con.OpenAsync();
        await using SqlCommand? cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(parameters);
        await using SqlDataReader? reader = await cmd.ExecuteReaderAsync();
        StringBuilder resultJson = new();
        if (!reader.HasRows) return;
        while (reader.Read())
        {
            resultJson.Append(reader.GetString(0));
        }

        FoodProduct? product = new();
        
        try
        {
            product = JsonSerializer.Deserialize<FoodProduct>(resultJson.ToString());
        }
        catch (JsonException)
        {
            Assert.Fail();
        }
        product.Should().NotBeNull();        
    }

    [Test]
    public async Task ExecuteQueryAsyncShouldWork()
    {
        const string sql =  "SELECT * FROM FoodProducts FP WHERE FP.ProductId = @ProductId";
        var foodProductList = await cherqueQuery.ExecuteQueryAsync<FoodProduct>(sql, parameters, connectionString);
        foodProductList.Should().NotBeNull();
    }    
    
    // Currently this is a fail.
    [Test]
    public async Task ExecuteQueryAsyncShouldWorkWithPrimitiveTypes()
    {
        const string sql =  "SELECT FP.ProductId FROM FoodProducts FP WHERE FP.ProductId = @ProductId";
        var foodProductList = await cherqueQuery.ExecuteQueryAsync<int>(sql, parameters, connectionString);
        foodProductList[0].Should().Be(1);
    }

    [Test]
    public async Task MapToPrimitiveTypes<T>()
    {
        const string sql = "SELECT FP.ProductId FROM FoodProducts FP WHERE FP.ProductId = @ProductId";
        await using SqlConnection con = new(connectionString);
        await con.OpenAsync();
        await using SqlCommand cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(parameters);
        await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        
    }
}