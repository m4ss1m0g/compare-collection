namespace Collection.Comparer.Models;

/// <summary>
/// Contains the compare result of Compare class
/// </summary>
/// <typeparam name="T">The initial enumerable type</typeparam>
public class CompareResult<T>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="deleted">Deleted items</param>
    /// <param name="changed">Changed items</param>
    /// <param name="inserted">Inserted items</param>
    public CompareResult(IEnumerable<T> deleted, IEnumerable<Tuple<T, T>> changed, IEnumerable<T> inserted)
    {
        Deleted = deleted;
        Changed = changed;
        Inserted = inserted;
    }

    /// <summary>
    /// The list of deleted items
    /// </summary>
    public IEnumerable<T> Deleted { get; }

    /// <summary>
    /// The list of changed items
    /// </summary>
    public IEnumerable<Tuple<T, T>> Changed { get; }

    /// <summary>
    /// The list of inserted items
    /// </summary>
    public IEnumerable<T> Inserted { get; }
}