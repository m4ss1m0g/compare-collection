using System.Data;
using System.Text;
using Newtonsoft.Json;

namespace Collection.Comparer.Models;

public class InMemoryCache
{
    private static readonly object _locker = new();

    private readonly Dictionary<string, object> _cache = new();

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

    public CompareResult<T, T> GetChanges<T, TKey>(
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
        return CompareCollection(cachedList, current, keySelector, compareFunc);

        // return new ChangeResult<T, T>(new List<T>(), new List<Tuple<T, T>>(), new List<T>());
    }

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
        var changes = CompareCollection(cachedList, current, keySelector, compareFunc);

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

    public bool ContainsKey(string key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _cache.ContainsKey(key);
    }

    private static CompareResult<TLocal, TLocal> CompareCollection<TLocal, TKey>(
            IEnumerable<TLocal> local,
            IEnumerable<TLocal> remote,
            Func<TLocal, TKey> keySelector,
            Func<TLocal, TLocal, bool>? compareFunc = null) where TKey : notnull
                                                            where TLocal : notnull
    {
        if (local is null)
        {
            throw new ArgumentNullException(nameof(local));
        }

        if (remote is null)
        {
            throw new ArgumentNullException(nameof(remote));
        }

        if (keySelector is null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var remoteKeyValues = remote.ToDictionary(keySelector);
        var localKeyValues = local.ToDictionary(keySelector);
        var changed = new List<Tuple<TLocal, TLocal>>();

        foreach (var localItem in local)
        {
            var localItemKey = keySelector(localItem);

            // Check if item is both in local and remote
            if (remoteKeyValues.Remove(localItemKey, out var remoteItemValue))
            {
                // The item is in both collections -> compare
                var isItemChanged = compareFunc != null ? !compareFunc(localItem, remoteItemValue) :
                 !(MD5(localItem) == MD5(remoteItemValue));

                if (isItemChanged)
                {
                    // Set as changed
                    changed.Add(new Tuple<TLocal, TLocal>(localItem, remoteItemValue));
                }

                // The remote and local are equal
                localKeyValues.Remove(localItemKey);
            }

            // If item is not in remote list means it has been removed
        }

        var deleted = localKeyValues.Values;
        var inserted = remoteKeyValues.Values;

        return new CompareResult<TLocal, TLocal>(deleted, changed, inserted);
    }

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

    private static string MD5<T>(T source)
    {
        var json = JsonConvert.SerializeObject(source);
        using var provider = System.Security.Cryptography.MD5.Create();
        StringBuilder builder = new();

        foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(json)))
            builder.Append(b.ToString("x2").ToLower());

        return builder.ToString();
    }

    private static T Clone<T>(T item, FastDeepCloner.FieldType fieldType = FastDeepCloner.FieldType.PropertyInfo)
    {
        lock (_locker)
        {
            return (T)FastDeepCloner.DeepCloner.Clone(item, fieldType);
        }
    }
}
