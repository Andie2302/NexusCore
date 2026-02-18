namespace NexusCore;

public interface IQuantifiedProductEntry
{
    Guid ProductUuid { get; init; }
    int Quantity { get; set; }
}