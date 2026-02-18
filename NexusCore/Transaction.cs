namespace NexusCore;

public class Transaction
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public List<TransactionEntry> Items { get; init; } = [];
    public string PreviousReceiptHash { get; set; } = string.Empty;
    public string CurrentReceiptHash { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public decimal GrandTotal => Items.Sum(i => i.Total);
}