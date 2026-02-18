namespace NexusCore;

public class ProductCartEntry : IQuantifiedProductEntry
{
    public Guid ProductUuid { get; init; }
    public int Quantity { get; set; }
}