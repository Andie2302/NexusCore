namespace NexusCore;

public class InventoryEntry : IQuantifiedProductEntry
{
    public Guid ProductUuid { get; init; }
    public int Quantity { get; set; }
}