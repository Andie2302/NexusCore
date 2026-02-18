namespace NexusCore;

public class Product
{
    public Guid Uuid { get; init; }
    public string Sku { get; init; }
    public string Name { get; set; }
    //######################################
    public decimal Price { get; set; }
    public int TaxCategoryId { get; set; }
}

public class ProductCatalog
{
    private readonly Dictionary<Guid, Product> _productsById = new();
    private readonly Dictionary<string, Guid> _skuToUuidMap = new(StringComparer.OrdinalIgnoreCase);

    public void AddProduct(Product product)
    {
        _productsById[product.Uuid] = product;
        _skuToUuidMap[product.Sku] = product.Uuid;
    }

    public void RemoveProduct(Guid uuid)
    {
        if (!_productsById.TryGetValue(uuid, out var product)) return;
        _skuToUuidMap.Remove(product.Sku);
        _productsById.Remove(uuid);
    }

    public void Clear()
    {
        _productsById.Clear();
        _skuToUuidMap.Clear();
    }

    public Product? GetBySku(string sku) => _skuToUuidMap.TryGetValue(sku, out var uuid) ? _productsById[uuid] : null;
    public bool ContainsSku(string sku) => _skuToUuidMap.ContainsKey(sku);
    public bool ContainsUuid(Guid uuid) => _productsById.ContainsKey(uuid);
    public Product? GetById(Guid id) => _productsById.GetValueOrDefault(id);
    public IEnumerable<Product> GetAll() => _productsById.Values;
    public int Count => _productsById.Count;
}

public class InventoryEntry
{
    public Guid ProductUuid { get; init; }
    public int Quantity { get; set; }
}

public class Inventory
{
    private readonly Dictionary<Guid, InventoryEntry> _entries = new();
    public InventoryEntry? GetEntry(Guid productUuid) => _entries.GetValueOrDefault(productUuid);
    public void AddEntry(Guid productUuid, InventoryEntry entry) => _entries[productUuid] = entry;
    public void RemoveEntry(Guid productUuid) => _entries.Remove(productUuid);
    public IEnumerable<InventoryEntry> GetAllEntries() => _entries.Values;
    public int Count => _entries.Count;
    public bool Contains(Guid productUuid) => _entries.ContainsKey(productUuid);
    public void Clear() => _entries.Clear();

    public void UpdateEntryQuantity(Guid productUuid, int quantity)
    {
        if (_entries.TryGetValue(productUuid, out var entry))
        {
            entry.Quantity = quantity;
        }
        else
        {
            AddEntry(productUuid, new InventoryEntry() { ProductUuid = productUuid, Quantity = quantity });
        }
    }

    public int GetEntryQuantity(Guid productUuid) => _entries[productUuid].Quantity;
}

public class ProductCartEntry
{
    public Guid ProductUuid { get; init; }
    public int Quantity { get; set; }
}

public class ProductCart
{
    private readonly Dictionary<Guid, ProductCartEntry> _entries = new();
    public ProductCartEntry? GetEntry(Guid productUuid) => _entries.GetValueOrDefault(productUuid);
    public void AddEntry(Guid productUuid, ProductCartEntry entry) => _entries[productUuid] = entry;
    public void RemoveEntry(Guid productUuid) => _entries.Remove(productUuid);
    public IEnumerable<ProductCartEntry> GetAllEntries() => _entries.Values;
    public int Count => _entries.Count;
    public bool Contains(Guid productUuid) => _entries.ContainsKey(productUuid);
    public void Clear() => _entries.Clear();

    public void UpdateEntryQuantity(Guid productUuid, int quantity)
    {
        if (_entries.TryGetValue(productUuid, out var entry))
        {
            entry.Quantity = quantity;
        }
        else
        {
            AddEntry(productUuid, new ProductCartEntry { ProductUuid = productUuid, Quantity = quantity });
        }
    }

    public int GetEntryQuantity(Guid productUuid) => _entries[productUuid].Quantity;
}

public class TransactionEntry
{
    public Guid OrderUuid { get; init; }
    public DateTime Timestamp { get; init; }
    //#######################################
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
    private readonly ProductCatalog _catalog;
    private readonly Inventory _inventory;

    public PosEngine(ProductCatalog catalog, Inventory inventory)
    {
        _catalog = catalog;
        _inventory = inventory;
    }

    public Transaction CreateTransaction(ProductCart cart, string previousHash)
    {
        var transaction = new Transaction
        {
            PreviousReceiptHash = previousHash
        };

        foreach (var cartEntry in cart.GetAllEntries())
        {
            var product = _catalog.GetById(cartEntry.ProductUuid);
            if (product == null) throw new Exception("Produkt im Katalog nicht gefunden!");

            // Snapshot erstellen
            var receiptItem = new TransactionEntry
            {
                ProductUuid = product.Uuid,
                NameAtSale = product.Name,
                PriceAtSale = product.Price,
                Quantity = cartEntry.Quantity,
                TaxCategoryIdAtSale = product.TaxCategoryId
            };

            transaction.Items.Add(receiptItem);

            // Lagerbestand abziehen
            int currentStock = _inventory.GetEntryQuantity(product.Uuid);
            _inventory.UpdateEntryQuantity(product.Uuid, currentStock - cartEntry.Quantity);
        }

        // Hier würde man jetzt den Hash berechnen (für später)
        transaction.CurrentReceiptHash = "DUMMY_HASH_" + transaction.Id;

        return transaction;
    }
}