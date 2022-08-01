# Compare Enumerables

This small utility can compare two `Enumerable` and get the differences.
You can use it for know what items/rows/entities was added, updated or removed from the list than what are in cache.

The function accept two enumerables, a function to get the key and and an optionally comapre function, otherwise it use the build-in MD5.

## Basic usage

You can use directly the comparer to get the differences

``` csharp

class Foo
{
    public int Id { get; set; }

    public string? Title { get; set; }
}

var list1 = new List<Foo>();
list1.Add(new Foo{Id = 1, Title = "One" });
list1.Add(new Foo{Id = 2, Title = "Two" });

var list2 = new List<Foo>();
// list2.Add(new Foo{Id = 1, Title = "One" });       // deleted
list2.Add(new Foo{Id = 2, Title = "Two modified" }); // modified
list2.Add(new Foo{Id = 3, Title = "Three" });        // Inserted
list2.Add(new Foo{Id = 4, Title = "Four" });         // Inserted

// you can pass a custom function to check the differences or use built-in MD5 
var result = CompareEnumerables.GetChanges(list1, list2, p => p.Id); 

Assert.Equal(1, result.Deleted.Count());
Assert.Equal(1, result.Changed.Count());
Assert.Equal(2, result.Inserted.Count());

```

## Advanced usage

You can use an InMemory cache to _mimic_ Entity Framework when persist data.

``` csharp

class Foo
{
    public int Id { get; set; }

    public string? Title { get; set; }
}

// suppose to get data using dapper or other micro-orm and create a list
var list1 = connection.GetAsync<Foo>("SELECT ID, TITLE FROM CUSTOMERS");

var cache = new InMemoryCache();
cache.Add("MyKey", list1);

/* 
    code that change the list1 then you can get diffences between the actual list and the cached list
*/

// Method One: get the differences and loop through the results lists
var differences = cache.GetChanges("MyKey", list1, p=> p.Id);

// Method two: you can pass action for each of the change types
cache.GetChages("MyKey", list, 
                (o,n)=> // onUpdate -- n == new item , o old item,
                (n)=>   // onInsert -- n == new item
                (d)=>   // onDelete -- d == deleted item
                );


```

## Bonus

There also an extension method to get differences from cache if any or insert all.

> Note: the enumerable wont be added to the cache

``` csharp

class Foo
{
    public int Id { get; set; }

    public string? Title { get; set; }
}

// suppose to get data using dapper or other micro-orm and create a list
var list1 = connection.GetAsync<Foo>("SELECT ID, TITLE FROM CUSTOMERS");

/* 
    code that change the list1 then you can get diffences between the actual list and the cached list
*/

// if find the list inside cache start comparison, otherwise call the onInsert action
var cache = new InMemoryCache();
cache.GetChagesOrInsert("MyKey", 
                list, 
                (o,n)=> // onUpdate -- n == new item , o old item,
                (n)=>   // onInsert -- n == new item
                (d)=>   // onDelete -- d == deleted item
                );
```
