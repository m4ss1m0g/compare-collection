using Collection.Comparer.Models;

namespace Collection.Comparer.Extensions;

public static class CollectionExtensions
{
    public static void GetChangesOrInsert<T, TKey>(
        this CompareEnumerables cache,
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
            foreach (var item in current)
            {
                onInsert(item);
            }
        }
    }
}