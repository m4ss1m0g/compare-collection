namespace Collection.Comparer.Models;

public sealed class CompareResult<TLocal, TRemote>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="deleted">Deleted items</param>
    /// <param name="changed">Changed items</param>
    /// <param name="inserted">Inserted items</param>
    public CompareResult(IEnumerable<TLocal> deleted, IEnumerable<Tuple<TLocal, TRemote>> changed, IEnumerable<TRemote> inserted)
    {
        Deleted = deleted;
        Changed = changed;
        Inserted = inserted;
    }

    public IEnumerable<TLocal> Deleted { get; }

    public IEnumerable<Tuple<TLocal, TRemote>> Changed { get; }

    public IEnumerable<TRemote> Inserted { get; }
}