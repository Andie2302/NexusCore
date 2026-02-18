namespace NexusCore;

public class Product
{
    public Guid Uuid { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int TaxCategoryId { get; set; }
}

// ... existing code ...