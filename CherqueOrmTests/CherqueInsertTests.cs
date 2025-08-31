using System.Reflection;
using AwesomeAssertions;

namespace CherqueOrmTests;

[TestFixture]
public class CherqueInsertTests
{
    private FoodProduct _foodProduct;

    [SetUp]
    public void Setup()
    {
        _foodProduct = new FoodProduct
        {
            ProductName = "Test Product",
            CategoryId = 1
        };
    }

    [Test]
    public void PropertiesShouldBeSet()
    {
        _foodProduct.ProductName.Should().Be("Test Product");
        _foodProduct.CategoryId.Should().Be(1);
    }

    [Test]
    public void GetTypeGetPropertiesShouldBeSet()
    {
        _foodProduct.GetType().GetProperties().Should().HaveCount(13);
    }

    [Test]
    public void GetTypeGetPropertiesShouldHaveCorrectNames()
    {
        _foodProduct.GetType().GetProperties().Select(x => x.Name).ToHashSet().Should().Contain(
            new HashSet<string>
            {
                "ProductId",
                "ProductName",
                "CategoryId",
                "NetQuantityUnit",
                "Price",
                "DateAdded",
                "LastUpdated",
                "Notes",
                "Barcode",
                "NetQuantityValue",
                "Nutrition",
                "Category"
            });
    }
    
    [Test]
    public void ReturnedTypePropertiesShouldBeSet()
    {
        PropertyInfo[] props = _foodProduct.GetType().GetProperties();
        HashSet<string> propertyNames = [];
        foreach (PropertyInfo prop in props)
        {
            if (prop.GetValue(_foodProduct) != null)
            {
                propertyNames.Add(prop.Name);
            }
        }

        propertyNames.Should().Contain("ProductName", "CategoryId");
    }

    [Test]
    public void ReturnedTypePropertiesShouldBeExcludedIfNullOrWhitespace()
    {
        _foodProduct.ProductName = " ";
        PropertyInfo[] props = _foodProduct.GetType().GetProperties();
        HashSet<string> propertyNames = [];
        foreach (PropertyInfo prop in props)
        {
            object? value = prop.GetValue(_foodProduct);
            if (value is not null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                propertyNames.Add(prop.Name);
            }
        }

        propertyNames.Should().NotContain("ProductName");
    }
    
    
}