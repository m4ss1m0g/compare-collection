using Bogus;
using Collection.Comparer.Models;

namespace Collection.Comparer.Test.Models;

public class ComparerEnumerablesTests
{
    class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Department { get; set; }
    }

    [Fact]
    public void Should_add_a_new_key_and_check_it()
    {
        // Arrange
        var cache = new InMemoryCache();

        // Act
        cache.Add("K1", new List<string>());

        // Assert
        cache.ContainsKey("K1");
    }

    [Fact]
    public void Should_getchanges_from_cache()
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
        cache.Add("Key1", list1);

        // Act
        list1.Add(testProducts.Generate()); // add new one
        list1.Add(testProducts.Generate()); // add new one
        list1[0].Department = "new department"; // modify
        list1[1].Department = "new department"; // modify
        list1[2].Department = "new department"; // modify
        list1.RemoveAll(p => p.Id == list1[3].Id); // remove one

        var result = cache.GetChanges("Key1", list1, p => p.Id);

        // Assert
        Assert.Single(result.Deleted);
        Assert.Equal(3, result.Changed.Count());
        Assert.Equal(2, result.Inserted.Count());
    }

    [Fact]
    public void Should_getchanges_and_call_actions()
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
        cache.Add("Key1", list1);

        // Act
        list1.Add(testProducts.Generate()); // add new one
        list1.Add(testProducts.Generate()); // add new one
        list1[0].Department = "new department"; // modify
        list1[1].Department = "new department"; // modify
        list1[2].Department = "new department"; // modify
        list1.RemoveAll(p => p.Id == list1[3].Id); // remove one

        int deleted = 0;
        int inserted = 0;
        int updated = 0;
        cache.GetChanges("Key1", list1, p => p.Id, (o,n)=> updated++, (n)=> inserted++, (d)=> deleted++);

        // Assert
        Assert.Equal(1, deleted);
        Assert.Equal(3, updated);
        Assert.Equal(2, inserted);
    }
}