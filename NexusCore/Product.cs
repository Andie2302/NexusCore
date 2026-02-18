namespace NexusCore;

public class Product
{
    public Guid Uuid { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int TaxCategoryId { get; set; }
}

public class ProductCatalog
{
    private readonly Dictionary<Guid, Product> _productsByUuid = new();
    private readonly Dictionary<string, Guid> _uuidBySku = new(StringComparer.OrdinalIgnoreCase);

    public void AddProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Uuid == Guid.Empty)
            throw new ArgumentException("Product UUID must not be empty.", nameof(product));

        if (string.IsNullOrWhiteSpace(product.Sku))
            throw new ArgumentException("Product SKU must not be empty.", nameof(product));

        // Ensure SKU map stays consistent if the same UUID is re-added with a different SKU.
        if (_productsByUuid.TryGetValue(product.Uuid, out var existing))
            _uuidBySku.Remove(existing.Sku);

        _productsByUuid[product.Uuid] = product;
        _uuidBySku[product.Sku] = product.Uuid;
    }

    public void RemoveProduct(Guid uuid)
    {
        if (!_productsByUuid.TryGetValue(uuid, out var product)) return;

        _uuidBySku.Remove(product.Sku);
        _productsByUuid.Remove(uuid);
    }

    public void Clear()
    {
        _productsByUuid.Clear();
        _uuidBySku.Clear();
    }

    public Product? GetBySku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku)) return null;
        return _uuidBySku.TryGetValue(sku, out var uuid) ? _productsByUuid.GetValueOrDefault(uuid) : null;
    }

    public bool ContainsSku(string sku) => !string.IsNullOrWhiteSpace(sku) && _uuidBySku.ContainsKey(sku);
    public bool ContainsUuid(Guid uuid) => _productsByUuid.ContainsKey(uuid);

    public Product? GetById(Guid uuid) => _productsByUuid.GetValueOrDefault(uuid);

    public IEnumerable<Product> GetAll() => _productsByUuid.Values;

    public int Count => _productsByUuid.Count;
}

public interface IQuantifiedProductEntry
{
    Guid ProductUuid { get; init; }
    int Quantity { get; set; }
}

public class InventoryEntry : IQuantifiedProductEntry
{
    public Guid ProductUuid { get; init; }
    public int Quantity { get; set; }
}

public class ProductCartEntry : IQuantifiedProductEntry
{
    public Guid ProductUuid { get; init; }
    public int Quantity { get; set; }
}

internal sealed class ProductEntryCollection<TEntry> where TEntry : class, IQuantifiedProductEntry
{
    private readonly Dictionary<Guid, TEntry> _entries = new();

    public TEntry? GetEntry(Guid productUuid) => _entries.GetValueOrDefault(productUuid);

    public IEnumerable<TEntry> GetAllEntries() => _entries.Values;

    public int Count => _entries.Count;

    public bool Contains(Guid productUuid) => _entries.ContainsKey(productUuid);

    public void Clear() => _entries.Clear();

    public void RemoveEntry(Guid productUuid) => _entries.Remove(productUuid);

    public void AddOrReplaceEntry(Guid productUuid, TEntry entry) => _entries[productUuid] = entry;

    public void SetQuantity(Guid productUuid, int quantity, Func<Guid, int, TEntry> entryFactory)
    {
        if (_entries.TryGetValue(productUuid, out var entry))
        {
            entry.Quantity = quantity;
            return;
        }

        _entries[productUuid] = entryFactory(productUuid, quantity);
    }

    public int GetRequiredQuantity(Guid productUuid)
    {
        if (!_entries.TryGetValue(productUuid, out var entry))
            throw new KeyNotFoundException($"No entry found for product UUID '{productUuid}'.");

        return entry.Quantity;
    }
}

public class Inventory
{
    private readonly ProductEntryCollection<InventoryEntry> _entries = new();

    public InventoryEntry? GetEntry(Guid productUuid) => _entries.GetEntry(productUuid);
    public void AddEntry(Guid productUuid, InventoryEntry entry) => _entries.AddOrReplaceEntry(productUuid, entry);
    public void RemoveEntry(Guid productUuid) => _entries.RemoveEntry(productUuid);
    public IEnumerable<InventoryEntry> GetAllEntries() => _entries.GetAllEntries();
    public int Count => _entries.Count;
    public bool Contains(Guid productUuid) => _entries.Contains(productUuid);
    public void Clear() => _entries.Clear();

    public void UpdateEntryQuantity(Guid productUuid, int quantity) =>
        _entries.SetQuantity(productUuid, quantity, (id, q) => new InventoryEntry { ProductUuid = id, Quantity = q });

    public int GetEntryQuantity(Guid productUuid) => _entries.GetRequiredQuantity(productUuid);
}

public class ProductCart
{
    private readonly ProductEntryCollection<ProductCartEntry> _entries = new();

    public ProductCartEntry? GetEntry(Guid productUuid) => _entries.GetEntry(productUuid);
    public void AddEntry(Guid productUuid, ProductCartEntry entry) => _entries.AddOrReplaceEntry(productUuid, entry);
    public void RemoveEntry(Guid productUuid) => _entries.RemoveEntry(productUuid);
    public IEnumerable<ProductCartEntry> GetAllEntries() => _entries.GetAllEntries();
    public int Count => _entries.Count;
    public bool Contains(Guid productUuid) => _entries.Contains(productUuid);
    public void Clear() => _entries.Clear();

    public void UpdateEntryQuantity(Guid productUuid, int quantity) =>
        _entries.SetQuantity(productUuid, quantity, (id, q) => new ProductCartEntry { ProductUuid = id, Quantity = q });

    public int GetEntryQuantity(Guid productUuid) => _entries.GetRequiredQuantity(productUuid);
}

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

public class PosEngine
{
    private const string ReceiptHashPrefix = "DUMMY_HASH_";

    private readonly ProductCatalog _catalog;
    private readonly Inventory _inventory;

    public PosEngine(ProductCatalog catalog, Inventory inventory)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
    }

    public Transaction CreateTransaction(ProductCart cart, string previousHash)
    {
        ArgumentNullException.ThrowIfNull(cart);
        previousHash ??= string.Empty;

        var transaction = new Transaction
        {
            PreviousReceiptHash = previousHash
        };

        foreach (var cartEntry in cart.GetAllEntries())
        {
            if (cartEntry.Quantity <= 0)
                throw new InvalidOperationException($"Cart contains non-positive quantity for product UUID '{cartEntry.ProductUuid}'.");

            var product = GetRequiredProduct(cartEntry.ProductUuid);

            transaction.Items.Add(CreateTransactionEntry(product, cartEntry.Quantity));

            var currentStock = _inventory.GetEntryQuantity(product.Uuid);
            var newStock = currentStock - cartEntry.Quantity;

            if (newStock < 0)
                throw new InvalidOperationException($"Insufficient stock for SKU '{product.Sku}'. Current: {currentStock}, requested: {cartEntry.Quantity}.");

            _inventory.UpdateEntryQuantity(product.Uuid, newStock);
        }

        transaction.CurrentReceiptHash = ReceiptHashPrefix + transaction.Id;
        return transaction;
    }

    private Product GetRequiredProduct(Guid productUuid) =>
        _catalog.GetById(productUuid)
        ?? throw new InvalidOperationException($"Product '{productUuid}' was not found in the catalog.");

    private static TransactionEntry CreateTransactionEntry(Product product, int quantity) =>
        new()
        {
            ProductUuid = product.Uuid,
            NameAtSale = product.Name,
            PriceAtSale = product.Price,
            Quantity = quantity,
            TaxCategoryIdAtSale = product.TaxCategoryId
        };
}