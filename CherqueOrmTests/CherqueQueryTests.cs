using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using CherqueOrm;
using Microsoft.Data.SqlClient;

namespace CherqueOrmTests;

public class Tests
{
    string connectionString = string.Empty;

    public record class Category
    {
        [Key] // Denotes this property as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Specifies that the database generates the ID
        public int CategoryId { get; set; }

        [Required] // Corresponds to NOT NULL in SQL
        [MaxLength(100)] // Corresponds to NVARCHAR(100)
        public string CategoryName { get; set; } = string.Empty;

        // Navigation property: A category can have many food products
        public ICollection<FoodProduct>? FoodProducts { get; set; }
    }

    /// <summary>
    /// Represents a food product definition.
    /// Corresponds to the 'FoodProducts' SQL table.
    /// </summary>
    public record FoodProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; } = string.Empty;

        // Foreign Key property for Category
        public int? CategoryId { get; set; } // Nullable as per ON DELETE SET NULL

        // Navigation property: A food product belongs to one category
        [ForeignKey(nameof(CategoryId))] // Explicitly links to CategoryId
        public Category? Category { get; set; } // Nullable if CategoryId can be null

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(50)]
        public string? Barcode { get; set; } // UNIQUE in SQL, handled by EF Core if configured

        public decimal? NetQuantityValue { get; set; } // DECIMAL(10, 2)
        
        [MaxLength(20)]
        public string? NetQuantityUnit { get; set; } // NVARCHAR(20)

        public decimal? Price { get; set; } // DECIMAL(10, 2)

        public DateTime DateAdded { get; set; } = DateTime.Now; // DATETIME, with default GETDATE()
        
        public DateTime LastUpdated { get; set; } = DateTime.Now; // DATETIME, with default GETDATE()

        [MaxLength] // Corresponds to NVARCHAR(MAX)
        public string? Notes { get; set; }

        // Navigation property: A food product has one nutrition entry (1:1 relationship)
        public ProductNutrition? Nutrition { get; set; }
    }

    /// <summary>
    /// Represents detailed nutritional information for a food product.
    /// Corresponds to the 'ProductNutrition' SQL table.
    /// </summary>
    public record ProductNutrition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NutritionId { get; set; }

        // Foreign Key property for FoodProduct (nullable as per ON DELETE SET NULL)
        public int? ProductId { get; set; } // Nullable as per ON DELETE SET NULL

        // Navigation property: A nutrition entry belongs to one food product
        [ForeignKey(nameof(ProductId))] // Explicitly links to ProductId
        public FoodProduct? FoodProduct { get; set; } // Nullable if ProductId can be null

        // Nutritional Information (per 100g or 100ml)
        public int? CaloriesKcal { get; set; }
        public decimal? FatGrams { get; set; } // DECIMAL(6, 2)
        public decimal? SaturatedFatGrams { get; set; }
        public decimal? CarbohydratesGrams { get; set; }
        public decimal? SugarsGrams { get; set; }
        public decimal? ProteinGrams { get; set; }
        public decimal? DietaryFibreGrams { get; set; }
        public decimal? SaltGrams { get; set; }

        // Vitamins
        public decimal? VitaminAUg { get; set; }
        public decimal? VitaminCMg { get; set; }
        public decimal? VitaminDUg { get; set; }
        public decimal? VitaminEMg { get; set; }
        public decimal? VitaminKUg { get; set; }
        public decimal? ThiaminMg { get; set; }
        public decimal? RiboflavinMg { get; set; }
        public decimal? NiacinMg { get; set; }
        public decimal? VitaminB6Mg { get; set; }
        public decimal? FolateUg { get; set; }
        public decimal? VitaminB12Ug { get; set; }
        public decimal? BiotinUg { get; set; }
        public decimal? PantothenicAcidMg { get; set; }

        // Minerals
        public decimal? CalciumMg { get; set; }
        public decimal? IronMg { get; set; }
        public decimal? MagnesiumMg { get; set; }
        public decimal? PhosphorusMg { get; set; }
        public decimal? PotassiumMg { get; set; }
        public decimal? ZincMg { get; set; }
        public decimal? CopperMg { get; set; }
        public decimal? SeleniumUg { get; set; }
        public decimal? IodineUg { get; set; }
        public decimal? ChlorideMg { get; set; }
        public decimal? SodiumMg { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
    
    
    [SetUp]
    public void Setup()
    {
        connectionString = "Server=localhost,1433;Database=GroceriesDB;User Id=sa;Password=4#%fuPb5q6*sVe;TrustServerCertificate=True;";
    }

    private List<SqlParameter> parameters = [
        new() { ParameterName = "ProductId", SqlDbType = SqlDbType.Int, Value = 1 }
    ];
    
    [Test]
    public void SqlParameterCollection_ShouldContainParametersAdded()
    {
        const string sql = "SELECT * FROM FoodProducts FP WHERE FP.ProductId = @ProductId";
        using SqlConnection con = new(connectionString);
        using SqlCommand cmd = new(sql, con);
        cmd.Parameters.AddRange([..parameters]);
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

    [Test]
    public async Task QueryResultShouldCorrectlyDeserialize()
    {
        const string sql = "SELECT * FROM FoodProducts FP WHERE FP.ProductId = @ProductId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER";
        await using SqlConnection con = new(connectionString);
        await con.OpenAsync();
        await using SqlCommand? cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange([..parameters]);
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
        var cherqueQuery = new CherqueQuery();
        const string sql =  "SELECT * FROM FoodProducts FP WHERE FP.ProductId = @ProductId";
        SqlParameter[] parameters = [new SqlParameter { ParameterName = "ProductId", SqlDbType = SqlDbType.Int, Value = 1 }];
        var foodProductList = await cherqueQuery.ExecuteQueryAsync<FoodProduct>(sql, parameters, connectionString);
        foodProductList.Should().NotBeNull();
    }
}