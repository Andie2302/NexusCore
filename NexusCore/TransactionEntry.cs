namespace NexusCore;

public class TransactionEntry
{
    public Guid OrderUuid { get; init; }
    public DateTime Timestamp { get; init; }
    public Guid ProductUuid { get; init; }
    public string NameAtSale { get; init; } = string.Empty;
    public decimal PriceAtSale { get; init; }
    public int Quantity { get; init; }
    public int TaxCategoryIdAtSale { get; init; }
    public decimal Total => PriceAtSale * Quantity;
}