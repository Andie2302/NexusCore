using NexusCore;

Console.WriteLine("Hello, World!");

var catalog = new ProductCatalog();
var inventory = new Inventory();
var engine = new PosEngine(catalog, inventory);

var kaffee = new Product 
{ 
    Uuid = Guid.CreateVersion7(), 
    Sku = "KAF-01", 
    Name = "Espresso", 
    Price = 2.50m, 
    TaxCategoryId = 1 
};
catalog.AddProduct(kaffee);

inventory.UpdateEntryQuantity(kaffee.Uuid, 100);

var tisch1 = new ProductCart();
tisch1.UpdateEntryQuantity(kaffee.Uuid, 2);

Console.WriteLine($"Warenkorb erstellt. Anzahl Artikel: {tisch1.Count}");

const string lastHash = "0000000000000000"; 
var finalTransaction = engine.CreateTransaction(tisch1, lastHash);

Console.WriteLine("--- KASSENBON ---");
Console.WriteLine($"Datum: {finalTransaction.Timestamp}");
Console.WriteLine($"Beleg-ID: {finalTransaction.Id}");
foreach (var item in finalTransaction.Items)
{
    Console.WriteLine($"{item.Quantity}x {item.NameAtSale} à {item.PriceAtSale:C} = {item.Total:C}");
}
Console.WriteLine($"GESAMT: {finalTransaction.GrandTotal:C}");
Console.WriteLine($"Lagerbestand nach Verkauf: {inventory.GetEntryQuantity(kaffee.Uuid)}");
