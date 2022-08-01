using Bogus;
using Collection.Comparer.Extensions;
using Collection.Comparer.Models;

namespace Collection.Comparer.Test.Models;

public class HelpersExtensionsTests
{
    class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Department { get; set; }
    }

    [Fact]
    public void Should_call_oninsert_for_empty_cache()
    {
        // Arrange
        var idx = 0;
        var testProducts = new Faker<Product>()
            .StrictMode(true)
            .RuleFor(p => p.Id, f => idx++)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Department, f => f.Commerce.Department())
            ;

        var list1 = testProducts.Generate(10);
        var cache = new InMemoryCache();


        // Act
        int deleted = 0;
        int inserted = 0;
        int updated = 0;
        cache.GetChangesOrInsert("K1", list1, p => p.Id, (o, n) => updated++, (n) => inserted++, (d) => deleted++);

        // Assert
        Assert.Equal(0, deleted);
        Assert.Equal(0, updated);
        Assert.Equal(list1.Count, inserted);
    }
}