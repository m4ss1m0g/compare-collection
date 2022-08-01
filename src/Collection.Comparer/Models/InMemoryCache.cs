using System.Data;

namespace Collection.Comparer.Models;

/// <summary>
/// Cache enumerables and compare 
/// </summary>
public class InMemoryCache
{
    /// <summary>
    /// Used when cloning for thread safe operation
    /// </summary>
    private static readonly object _locker = new();

    /// <summary>
    /// Working as a cache
    /// </summary>
    private readonly Dictionary<string, object> _cache = new();

    /// <summary>
    /// Add an enumerable to the cache with specified key
    /// </summary>
    /// <remarks>This method CLONE the list, otherwise is a pointer</remarks>
    /// <typeparam name="T">The type of enumerable</typeparam>
    /// <param name="key">The key value</param>
    /// <param name="value">The enumerable value to cache</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="DuplicateNameException"></exception>
    public void Add<T>(string key, IEnumerable<T> value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (ContainsKey(key))
        {
            throw new DuplicateNameException($"Duplicate key {key}");
        }

        _cache.Add(key, Clone(value));
    }

    /// <summary>
    /// Get the changes between the current enumerable value and 
    /// the cached selected by the key
    /// </summary>
    /// <typeparam name="T">The enumerable type</typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="current">The current enumerable to compare to the value on cache</param>
    /// <param name="keySelector">The selector for comparing the values between the enumerables (i.e. Id)</param>
    /// <param name="compareFunc">The comparer function otherwise is MD5</param>
    /// <returns>The comparer result with inserted, updated and deleted items</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CompareResult<T> GetChanges<T, TKey>(
        string key,
        IEnumerable<T> current,
        Func<T, TKey> keySelector,
        Func<T, T, bool>? compareFunc = null)
                                                where T : notnull
                                                where TKey : notnull

    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (current is null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        if (keySelector is null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var cachedList = Get<T>(key);
        return CompareEnumerables.GetChanges(cachedList, current, keySelector, compareFunc);
    }

    /// <summary>
    /// Get the changes between the current enumerable value and 
    /// the cached selected by the key and call the respective Action
    /// for the compare result.
    /// </summary>
    /// <typeparam name="T">The enumerable type</typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="current">The current enumerable to compare to the value on cache</param>
    /// <param name="keySelector">The selector for comparing the values between the enumerables (i.e. Id)</param>
    /// <param name="onUpdate">The action to perform for each updated item</param>
    /// <param name="onInsert">The action to perform for each inserted item</param>
    /// <param name="onDelete">The action to perform for each deleted item</param>
    /// <param name="compareFunc">The comparer function otherwise is MD5</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void GetChanges<T, TKey>(
        string key,
        IEnumerable<T> current,
        Func<T, TKey> keySelector,
        Action<T, T> onUpdate,
        Action<T> onInsert,
        Action<T> onDelete,
        Func<T, T, bool>? compareFunc = null)
                                                where T : notnull
                                                where TKey : notnull
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (current is null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        if (keySelector is null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var cachedList = Get<T>(key)!;
        var changes = CompareEnumerables.GetChanges(cachedList, current, keySelector, compareFunc);

        foreach (var item in changes.Changed)
        {
            onUpdate(item.Item1, item.Item2);
        }

        foreach (var item in changes.Deleted)
        {
            onDelete(item);
        }

        foreach (var item in changes.Inserted)
        {
            onInsert(item);
        }
    }

    /// <summary>
    /// Check if the cache contains the specified key
    /// </summary>
    /// <param name="key">The key value</param>
    /// <returns>True if exists otherwise false</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool ContainsKey(string key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _cache.ContainsKey(key);
    }



    /// <summary>
    /// Retrieve from the cache the enumerable specified by the key
    /// </summary>
    /// <typeparam name="T">The enumerable type</typeparam>
    /// <param name="key">The key value</param>
    /// <returns>The enumerable</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    private IEnumerable<T> Get<T>(string key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (!ContainsKey(key))
        {
            throw new KeyNotFoundException(nameof(key));
        }

        var item = _cache[key];

        return (IEnumerable<T>)item;
    }

    /// <summary>
    /// Clone the item
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    /// <param name="item">The item to clone</param>
    /// <param name="fieldType">Field type value</param>
    /// <returns></returns>
    private static T Clone<T>(T item, FastDeepCloner.FieldType fieldType = FastDeepCloner.FieldType.Both)
    {
        lock (_locker)
        {
            return (T)FastDeepCloner.DeepCloner.Clone(item, fieldType);
        }
    }
}