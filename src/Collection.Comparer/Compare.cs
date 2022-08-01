using Collection.Comparer.Extensions;
using Collection.Comparer.Models;

namespace Collection.Comparer;

/// <summary>
/// Compare enumerables and return a change sets.
/// </summary>
public class Compare
{
    /// <summary>
    /// Compare two enumerables and return the compare result
    /// </summary>
    /// <typeparam name="T">The type of enumerable</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <param name="initial">The first enumerator</param>
    /// <param name="final">The second enumerator</param>
    /// <param name="keySelector">The selector for comparing the values between the enumerables (i.e. Id)</param>
    /// <param name="compareFunc">The comparer function otherwise is MD5</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static CompareResult<T> GetChanges<T, TKey>(
            IEnumerable<T> initial,
            IEnumerable<T> final,
            Func<T, TKey> keySelector,
            Func<T, T, bool>? compareFunc = null) where TKey : notnull
                                                            where T : notnull
    {
        if (initial is null)
        {
            throw new ArgumentNullException(nameof(initial));
        }

        if (final is null)
        {
            throw new ArgumentNullException(nameof(final));
        }

        if (keySelector is null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var finalKeyValues = final.ToDictionary(keySelector);
        var initialKeyValues = initial.ToDictionary(keySelector);
        var changed = new List<Tuple<T, T>>();

        foreach (var initialItem in initial)
        {
            var initialItemKey = keySelector(initialItem);

            // Check if item is both in initial and final otherwise it has been removed
            if (finalKeyValues.Remove(initialItemKey, out var finalItemValue))
            {
                // The item is in both collections -> compare
                var isItemChanged = compareFunc != null ? !compareFunc(initialItem, finalItemValue) :
                 !(initialItem.MD5() == finalItemValue.MD5());

                if (isItemChanged)
                {
                    // Set as changed
                    changed.Add(new Tuple<T, T>(initialItem, finalItemValue));
                }

                // The initial and final are equal
                initialKeyValues.Remove(initialItemKey);
            }
        }

        var deleted = initialKeyValues.Values;
        var inserted = finalKeyValues.Values;

        return new CompareResult<T>(deleted, changed, inserted);
    }
}
