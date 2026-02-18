namespace NexusCore;

public class ProductCatalog
{
    private readonly Dictionary<Guid, Product> _productsByUuid = new();
    private readonly Dictionary<string, Guid> _uuidBySku = new(StringComparer.OrdinalIgnoreCase);

    public void AddProduct(Product product)
    {
        ValidateProduct(product);

        // Ensure SKU map stays consistent if the same UUID is re-added with a different SKU.
        if (_productsByUuid.TryGetValue(product.Uuid, out var existing))
            _uuidBySku.Remove(existing.Sku);

        _productsByUuid[product.Uuid] = product;
        _uuidBySku[product.Sku] = product.Uuid;
    }

    private static void ValidateProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Uuid == Guid.Empty)
            throw new ArgumentException("Product UUID must not be empty.", nameof(product));

        if (string.IsNullOrWhiteSpace(product.Sku))
            throw new ArgumentException("Product SKU must not be empty.", nameof(product));
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

    public Product? GetByUuid(Guid uuid) => _productsByUuid.GetValueOrDefault(uuid);

    public IEnumerable<Product> GetAll() => _productsByUuid.Values;
    public int Count => _productsByUuid.Count;
}