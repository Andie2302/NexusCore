namespace NexusCore;

public class TransactionHistory
{
    private readonly List<Transaction> _transactions = [];

    public void Add(Transaction transaction) 
    {
        ArgumentNullException.ThrowIfNull(transaction);
        _transactions.Add(transaction);
    }

    // Liefert den Hash des letzten Bons oder einen Start-Wert (Seed)
    public string GetLastHash() => 
        _transactions.LastOrDefault()?.CurrentReceiptHash ?? "0000000000000000";

    public IEnumerable<Transaction> GetAll() => _transactions;
}