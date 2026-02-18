namespace NexusCore;

public sealed class ProductEntryCollection<TEntry> where TEntry : class, IQuantifiedProductEntry
{
    private readonly Dictionary<Guid, TEntry> _entries = new();

    public TEntry? GetEntry(Guid productUuid) => _entries.GetValueOrDefault(productUuid);
    public IEnumerable<TEntry> GetAllEntries() => _entries.Values;
    public int Count => _entries.Count;

    public bool Contains(Guid productUuid) => _entries.ContainsKey(productUuid);
    public void Clear() => _entries.Clear();
    public void RemoveEntry(Guid productUuid) => _entries.Remove(productUuid);

    public void AddOrReplaceEntry(Guid productUuid, TEntry entry) => _entries[productUuid] = entry;

    public void AddOrReplaceEntry(TEntry entry) => _entries[entry.ProductUuid] = entry;

    public void SetQuantity(Guid productUuid, int quantity, Func<Guid, int, TEntry> entryFactory)
    {
        if (_entries.TryGetValue(productUuid, out var entry))
        {
            entry.Quantity = quantity;
            return;
        }

        _entries[productUuid] = entryFactory(productUuid, quantity);
    }

    public int GetRequiredQuantity(Guid productUuid)
    {
        if (!_entries.TryGetValue(productUuid, out var entry))
            throw new KeyNotFoundException($"No entry found for product UUID '{productUuid}'.");

        return entry.Quantity;
    }
}