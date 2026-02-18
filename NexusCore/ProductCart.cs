namespace NexusCore;

public class ProductCart : QuantifiedProductCollection<ProductCartEntry>
{
    public void AddEntry(Guid productUuid, ProductCartEntry entry) => Entries.AddOrReplaceEntry(productUuid, entry);

    public void UpdateEntryQuantity(Guid productUuid, int quantity) =>
        UpdateEntryQuantity(productUuid, quantity, (id, q) => new ProductCartEntry { ProductUuid = id, Quantity = q });
}