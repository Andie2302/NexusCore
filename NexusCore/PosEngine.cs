namespace NexusCore;

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
        _catalog.GetByUuid(productUuid)
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