namespace NexusCore;

public class Inventory : QuantifiedProductCollection<InventoryEntry>
{
    public void AddEntry(Guid productUuid, InventoryEntry entry) => Entries.AddOrReplaceEntry(productUuid, entry);

    public void UpdateEntryQuantity(Guid productUuid, int quantity) =>
        UpdateEntryQuantity(productUuid, quantity, (id, q) => new InventoryEntry { ProductUuid = id, Quantity = q });
}