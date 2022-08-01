using Bogus;

namespace Collection.Comparer.Test;

public class CompareTests
{
    class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Department { get; set; }
    }

    [Fact]
    public void Should_compare_two_enumerables()
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
        var list2 = FastDeepCloner.DeepCloner.Clone(list1, FastDeepCloner.FieldType.Both);


        // Modify 4
        list2[0].Department = "New Department";
        list2[1].Department = "New Department";
        list2[2].Department = "New Department";
        list2[3].Department = "New Department";

        // Remove three
        list2.RemoveAll(p => p.Id == 5);
        list2.RemoveAll(p => p.Id == 6);
        list2.RemoveAll(p => p.Id == 7);

        // Add two new
        list2.AddRange(testProducts.Generate(2));

        // Act
        var result = Compare.GetChanges(list1, list2, p => p.Id);

        // Assert
        Assert.Equal(3, result.Deleted.Count());
        Assert.Equal(4, result.Changed.Count());
        Assert.Equal(2, result.Inserted.Count());
    }
}
