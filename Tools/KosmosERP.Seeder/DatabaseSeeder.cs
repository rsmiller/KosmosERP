using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;

namespace KosmosERP.Seeder;

/// <summary>
/// Seeds a coherent computer-parts manufacturer dataset. Split across partial files by module:
///   DatabaseSeeder.Products.cs — settings, lookups, vendors, products, BOMs (Milestone 1)
/// Later milestones add customers/CRM/sales, fulfillment/inventory, and accounting.
///
/// Rules (see plan): reuse <see cref="CommonDataHelper{T}"/> for audit fields, never set <c>id</c>
/// (MySQL auto-increments; ids are read back after SaveChanges for FKs), and the run is idempotent.
/// </summary>
public partial class DatabaseSeeder
{
    private readonly ERPDbContext _context;

    // The Settings.company_name that marks this dataset as already seeded.
    private const string SeedCompanyName = "Kosmos Computer Works";

    // The calling user id stamped on every seeded row's created_by/updated_by.
    private const int SystemUserId = 1;

    // External ids of the seeded salespeople (used for order attribution + reference loading).
    private static readonly string[] SeedSalespersonExternalIds = { "ext-jordan", "ext-riley", "ext-morgan" };

    // Cross-milestone lookups populated as we insert, so later tiers can resolve FKs.
    private readonly Dictionary<string, int> _vendorIdByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _productIdBySku = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, Product> _productById = new();
    private readonly Dictionary<string, int> _userIdByExternalId = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _salespersonExternalIds = new();
    private readonly Dictionary<int, int> _addressIdByCustomerId = new();
    private readonly Dictionary<int, int> _firstContactByCustomer = new();
    private readonly List<Customer> _customers = new();
    private readonly Dictionary<string, int> _accountIdByNumber = new();
    private readonly List<SeededOrder> _orders = new();
    private readonly List<SeededPO> _purchaseOrders = new();

    public DatabaseSeeder(ERPDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (_context.Settings.Any(s => s.company_name == SeedCompanyName))
        {
            Console.WriteLine($"Database already seeded (found Settings company '{SeedCompanyName}'). Skipping.");
            return;
        }

        Console.WriteLine("Seeding computer-parts manufacturer dataset...");

        await SeedSettingsAsync();
        await SeedProductCategoriesAsync();
        await SeedVendorsAsync();
        await SeedProductsAsync();
        await SeedBomsAsync();

        await SeedLookupsAsync();

        LoadReferenceData();

        await SeedUsersAndRolesAsync();
        await SeedCustomersAsync();
        await SeedCrmAsync();
        await SeedSalesOrdersAsync();

        await SeedPurchaseOrdersAsync();
        await SeedProductionOrdersAsync();
        await SeedShipmentsAsync();
        await SeedInventoryAsync();

        await SeedChartOfAccountsAsync();
        await SeedInvoicesAndPaymentsAsync();

        Console.WriteLine("Seeding complete.");
        Console.WriteLine($"  Vendors:        {_vendorIdByName.Count}");
        Console.WriteLine($"  Products:       {_productIdBySku.Count}");
        Console.WriteLine($"  BOM rows:       {_context.BOMs.Count()}");
        Console.WriteLine($"  Customers:      {_context.Customers.Count()}");
        Console.WriteLine($"  Sales orders:   {_orders.Count}");
        Console.WriteLine($"  Purchase orders:{_purchaseOrders.Count}");
        Console.WriteLine($"  Shipments:      {_context.ShipmentHeaders.Count()}");
        Console.WriteLine($"  AR invoices:    {_context.ARInvoiceHeaders.Count()}");
        Console.WriteLine($"  AP invoices:    {_context.APInvoiceHeaders.Count()}");
        Console.WriteLine($"  Inventory rows: {_context.InventoryCounts.Count()}");
        Console.WriteLine($"  GL postings:    {_context.FinancialTransactions.Count()}");
    }

    /// <summary>Stamps audit fields (created/updated by + timestamps) on a row before insert.</summary>
    private static T Stamp<T>(T model) where T : BaseDatabaseModel
        => CommonDataHelper<T>.FillCommonFields(model, SystemUserId);

    /// <summary>Stamps audit fields with a specific created_by/updated_by (e.g. a salesperson's external id).</summary>
    private static T StampAs<T>(T model, string userId) where T : BaseDatabaseModel
        => CommonDataHelper<T>.FillCommonFields(model, userId);

    /// <summary>
    /// Populates the in-memory vendor/product lookups from rows already in the database, so
    /// Milestones 2–4 can run even when the Milestone 1 seed methods are skipped. Uses indexer
    /// assignment so it is safe to call whether or not those methods also populated the lookups.
    /// </summary>
    private void LoadReferenceData()
    {
        foreach (var vendor in _context.Vendors.Where(v => !v.is_deleted).ToList())
            _vendorIdByName[vendor.vendor_name] = vendor.id;

        foreach (var product in _context.Products.Where(p => !p.is_deleted).ToList())
        {
            _productIdBySku[product.identifier1] = product.id;
            _productById[product.id] = product;
        }

        foreach (var user in _context.Users.Where(u => u.external_id != null).ToList())
            _userIdByExternalId[user.external_id] = user.id;

        foreach (var ext in SeedSalespersonExternalIds)
            if (_userIdByExternalId.ContainsKey(ext) && !_salespersonExternalIds.Contains(ext))
                _salespersonExternalIds.Add(ext);

        foreach (var ca in _context.CustomerAddresses.Where(c => !c.is_deleted).ToList())
            if (!_addressIdByCustomerId.ContainsKey(ca.customer_id))
                _addressIdByCustomerId[ca.customer_id] = ca.address_id;

        foreach (var contact in _context.Contacts.Where(c => !c.is_deleted).ToList())
            if (!_firstContactByCustomer.ContainsKey(contact.customer_id))
                _firstContactByCustomer[contact.customer_id] = contact.id;

        Console.WriteLine($"  Loaded reference data: {_vendorIdByName.Count} vendors, {_productById.Count} products, " +
                          $"{_userIdByExternalId.Count} users, {_addressIdByCustomerId.Count} customer addresses.");
    }

    /// <summary>Customers that have a resolvable ship-to address — the safe set for orders/CRM.</summary>
    private List<Customer> AddressableCustomers()
        => _context.Customers.Where(c => !c.is_deleted).ToList()
            .Where(c => _addressIdByCustomerId.ContainsKey(c.id)).ToList();

    // Lightweight carriers of created rows needed by later tiers.
    private sealed class SeededOrder
    {
        public int Id;
        public int CustomerId;
        public DateOnly OrderDate;
        public bool Complete;
        public bool Canceled;
        public string SalespersonExternalId;
        public readonly List<SeededOrderLine> Lines = new();
    }

    private sealed class SeededOrderLine
    {
        public int Id;
        public int ProductId;
        public int Qty;
        public decimal UnitPrice;
    }

    private sealed class SeededPO
    {
        public int Id;
        public int VendorId;
        public bool Complete;
        public bool Canceled;
        public DateTime CreatedOn;
        public readonly List<SeededPOLine> Lines = new();
    }

    private sealed class SeededPOLine
    {
        public int Id;
        public int ProductId;
        public int Qty;
        public decimal UnitPrice;
        public int ReceivedQty;
    }
}
