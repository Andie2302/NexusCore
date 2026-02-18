namespace NexusCore;

public abstract class QuantifiedProductCollection<TEntry>
    where TEntry : class, IQuantifiedProductEntry
{
    protected readonly ProductEntryCollection<TEntry> Entries = new();

    public TEntry? GetEntry(Guid productUuid) => Entries.GetEntry(productUuid);
    public void AddEntry(TEntry entry) => Entries.AddOrReplaceEntry(entry);
    public void RemoveEntry(Guid productUuid) => Entries.RemoveEntry(productUuid);
    public IEnumerable<TEntry> GetAllEntries() => Entries.GetAllEntries();

    public int Count => Entries.Count;
    public bool Contains(Guid productUuid) => Entries.Contains(productUuid);
    public void Clear() => Entries.Clear();

    protected void UpdateEntryQuantity(Guid productUuid, int quantity, Func<Guid, int, TEntry> entryFactory) =>
        Entries.SetQuantity(productUuid, quantity, entryFactory);

    public int GetEntryQuantity(Guid productUuid) => Entries.GetRequiredQuantity(productUuid);
}