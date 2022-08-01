using System.Text;
using Collection.Comparer.Models;
using Newtonsoft.Json;

namespace Collection.Comparer.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Get the changes between the collection as parameter and the one in cache if any.
    /// If not found any enumerable in cache threat all as insert
    /// </summary>
    /// <typeparam name="T">The type of enumerable</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <param name="cache">The cache</param>
    /// <param name="key">The key in cache</param>
    /// <param name="current">The current enumerable</param>
    /// <param name="keySelector">THe key selector</param>
    /// <param name="onUpdate">Action for update</param>
    /// <param name="onInsert">Action for insert</param>
    /// <param name="onDelete">Action for delete</param>
    /// <param name="compareFunc">The compare function</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetChangesOrInsert<T, TKey>(
        this InMemoryCache cache,
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
        if (cache is null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        if (cache.ContainsKey(key))
        {
            cache.GetChanges(key, current, keySelector, onUpdate, onInsert, onDelete, compareFunc);
        }
        else
        {
            // cache.Add(key, current);
            foreach (var item in current)
            {
                onInsert(item);
            }
        }
    }

    /// <summary>
    /// The MD5 hash function to serialize and compare the two list
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="value">The value to hash</param>
    /// <returns></returns>
    internal static string MD5<T>(this T value)
    {
        var json = JsonConvert.SerializeObject(value);
        using var provider = System.Security.Cryptography.MD5.Create();
        StringBuilder builder = new();

        foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(json)))
            builder.Append(b.ToString("x2").ToLower());

        return builder.ToString();
    }
}