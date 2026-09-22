using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.Seeder;

/// <summary>
/// Milestone 1 — foundation + product catalog for a computer-parts manufacturer:
/// company settings (the seed marker), product-category lookups, vendors, the product
/// catalog (finished PCs → sub-assemblies → components → consumables), and BOMs.
/// </summary>
public partial class DatabaseSeeder
{
    // Product category display names. Seeded as KeyValueStore rows (key == value) under the
    // ProductCategories group so the reports' category dropdown resolves cleanly, and set as
    // Product.category so the report filters (product.category == key) match.
    private static class Cat
    {
        public const string Cpu = "CPU";
        public const string Gpu = "GPU";
        public const string Motherboard = "Motherboard";
        public const string Memory = "Memory";
        public const string Storage = "Storage";
        public const string Psu = "PSU";
        public const string Case = "Case";
        public const string Cooling = "Cooling";
        public const string Cables = "Cables";
        public const string Fasteners = "Fasteners";
        public const string FinishedPc = "Finished PC";
        public const string SubAssembly = "Sub-Assembly";
        public const string Consumable = "Consumable";
    }

    // Product class labels (free text, shown on reports).
    private static class Cls
    {
        public const string Finished = "Finished Good";
        public const string Sub = "Sub-Assembly";
        public const string Component = "Component";
        public const string Consumable = "Consumable";
    }

    private async Task SeedSettingsAsync()
    {
        _context.Settings.Add(Stamp(new Settings
        {
            company_name = SeedCompanyName,
            company_address1 = "4200 Transistor Way",
            company_address2 = "Building C",
            company_city = "Austin",
            company_state = "TX",
            company_zip = "78744",
            company_country = "USA",
            company_phone = "512-555-0180",
            company_ar_email = "ar@kosmoscomputer.example",
            company_ap_email = "ap@kosmoscomputer.example",
            company_general_email = "hello@kosmoscomputer.example",
            company_website = "kosmoscomputer.example",
            tax_id = "74-1234567",
            fiscal_year_start = "01-01",
        }));

        await _context.SaveChangesAsync();
        Console.WriteLine("  Settings seeded.");
    }

    private async Task SeedProductCategoriesAsync()
    {
        var categories = new[]
        {
            Cat.Cpu, Cat.Gpu, Cat.Motherboard, Cat.Memory, Cat.Storage, Cat.Psu, Cat.Case,
            Cat.Cooling, Cat.Cables, Cat.Fasteners, Cat.FinishedPc, Cat.SubAssembly, Cat.Consumable,
        };

        foreach (var name in categories)
        {
            if (_context.KeyValueStores.Any(k => k.module_id == KeyValueIds.ProductCategories && k.key == name))
                continue;

            _context.KeyValueStores.Add(Stamp(new KeyValueStore
            {
                key = name,
                value = name,
                module_id = KeyValueIds.ProductCategories,
            }));
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  Product categories seeded ({categories.Length}).");
    }

    private async Task SeedVendorsAsync()
    {
        // (name, category label, phone, email, is_critical)
        var vendors = new (string Name, string Category, string Phone, string Email, bool Critical)[]
        {
            ("Silicon Path Components", "Components", "408-555-0110", "sales@siliconpath.example", true),
            ("MemoryWorks Distribution", "Memory", "408-555-0120", "orders@memoryworks.example", true),
            ("Nova Storage Supply", "Storage", "480-555-0130", "sales@novastorage.example", false),
            ("VoltEdge Power", "Power", "503-555-0140", "sales@voltedge.example", false),
            ("ChassisCraft Co", "Cases", "626-555-0150", "sales@chassiscraft.example", false),
            ("FrostByte Cooling", "Cooling", "512-555-0160", "sales@frostbyte.example", false),
            ("FastenRight Hardware", "Fasteners", "216-555-0170", "sales@fastenright.example", true),
            ("LinkLine Cables & Parts", "Cables", "312-555-0180", "sales@linkline.example", false),
        };

        var number = 1001;
        foreach (var v in vendors)
        {
            var vendor = Stamp(new Vendor
            {
                vendor_number = number++,
                vendor_name = v.Name,
                address_id = 0,
                phone = v.Phone,
                general_email = v.Email,
                category = v.Category,
                is_critial_vendor = v.Critical,
            });
            _context.Vendors.Add(vendor);
        }

        await _context.SaveChangesAsync();

        foreach (var vendor in _context.Vendors)
            _vendorIdByName[vendor.vendor_name] = vendor.id;

        Console.WriteLine($"  Vendors seeded ({vendors.Length}).");
    }

    private async Task SeedProductsAsync()
    {
        var products = new List<Product>();

        // Finished goods — manufactured, sellable, shippable. (sku, name, buildCost, salesPrice, stock)
        var finished = new (string Sku, string Name, decimal Cost, decimal Price, int Stock)[]
        {
            ("PC-NEBULA-01", "Nebula Gaming Tower", 980m, 1799m, 15),
            ("PC-ORION-01", "Orion Workstation", 1250m, 2299m, 10),
            ("PC-PULSAR-01", "Pulsar Mini PC", 540m, 999m, 20),
            ("PC-TITAN-1U", "Titan 1U Server", 1600m, 2999m, 8),
            ("PC-VEGA-01", "Vega Creator PC", 1100m, 1999m, 12),
        };
        foreach (var p in finished)
            products.Add(MakeProduct(p.Sku, p.Name, Cls.Finished, Cat.FinishedPc, "Silicon Path Components",
                ourCost: p.Cost, salesPrice: p.Price, listPrice: Math.Round(p.Price * 1.1m, 2),
                isManufactured: true, isMaterial: false, isSalesItem: true, isShippable: true,
                reqStock: p.Stock, reqReorder: Math.Max(2, p.Stock / 3), reqMin: 2, mfgMinutes: 180));

        // Sub-assemblies — manufactured in-house, consumed by finished-good BOMs. (sku, name, category, cost, stock)
        var subs = new (string Sku, string Name, decimal Cost, int Stock)[]
        {
            ("SUB-MOBO-ASSY", "Populated Motherboard Assembly", 420m, 25),
            ("SUB-PSU-ASSY", "PSU Module Assembly", 95m, 30),
            ("SUB-COOL-ASSY", "Cooling Loop Assembly", 70m, 30),
        };
        foreach (var p in subs)
            products.Add(MakeProduct(p.Sku, p.Name, Cls.Sub, Cat.SubAssembly, "Silicon Path Components",
                ourCost: p.Cost, salesPrice: 0m, listPrice: 0m,
                isManufactured: true, isMaterial: true, isSalesItem: false, isShippable: false,
                reqStock: p.Stock, reqReorder: p.Stock / 3, reqMin: 5, mfgMinutes: 45));

        // Components — purchased. (sku, name, category, vendorName, cost, stock)
        var components = new (string Sku, string Name, string Category, string Vendor, decimal Cost, int Stock)[]
        {
            ("CPU-RYZEN7", "Ryzen 7 CPU", Cat.Cpu, "Silicon Path Components", 260m, 40),
            ("CPU-COREI7", "Core i7 CPU", Cat.Cpu, "Silicon Path Components", 300m, 35),
            ("CPU-COREI5", "Core i5 CPU", Cat.Cpu, "Silicon Path Components", 190m, 45),
            ("GPU-RTX4070", "RTX 4070 GPU", Cat.Gpu, "Silicon Path Components", 540m, 25),
            ("GPU-RTX4060", "RTX 4060 GPU", Cat.Gpu, "Silicon Path Components", 320m, 30),
            ("MB-ATX-B650", "ATX B650 Motherboard", Cat.Motherboard, "Silicon Path Components", 165m, 40),
            ("MB-ITX-B660", "Mini-ITX B660 Motherboard", Cat.Motherboard, "Silicon Path Components", 175m, 30),
            ("MB-ATX-X670", "ATX X670 Motherboard", Cat.Motherboard, "Silicon Path Components", 240m, 20),
            ("RAM-16GB", "16GB DDR5 Module", Cat.Memory, "MemoryWorks Distribution", 45m, 120),
            ("RAM-32GB", "32GB DDR5 Module", Cat.Memory, "MemoryWorks Distribution", 85m, 80),
            ("SSD-1TB", "1TB NVMe SSD", Cat.Storage, "Nova Storage Supply", 70m, 90),
            ("SSD-2TB", "2TB NVMe SSD", Cat.Storage, "Nova Storage Supply", 120m, 60),
            ("HDD-4TB", "4TB HDD", Cat.Storage, "Nova Storage Supply", 85m, 50),
            ("PSU-650", "650W PSU Unit", Cat.Psu, "VoltEdge Power", 65m, 60),
            ("PSU-450", "450W PSU Unit", Cat.Psu, "VoltEdge Power", 45m, 55),
            ("CASE-ATX", "ATX Mid Tower Case", Cat.Case, "ChassisCraft Co", 70m, 40),
            ("CASE-ITX", "Mini-ITX Case", Cat.Case, "ChassisCraft Co", 60m, 30),
            ("CASE-1U", "1U Rackmount Chassis", Cat.Case, "ChassisCraft Co", 130m, 15),
            ("COOL-AIR", "Air CPU Cooler", Cat.Cooling, "FrostByte Cooling", 35m, 70),
            ("FAN-120", "120mm Case Fan", Cat.Cooling, "FrostByte Cooling", 9m, 200),
            ("CBL-POWER", "Internal Power Cable Set", Cat.Cables, "LinkLine Cables & Parts", 12m, 150),
            ("NIC-1G", "1GbE Network Card", Cat.Cables, "LinkLine Cables & Parts", 22m, 40),
        };
        foreach (var p in components)
            products.Add(MakeProduct(p.Sku, p.Name, Cls.Component, p.Category, p.Vendor,
                ourCost: p.Cost, salesPrice: 0m, listPrice: 0m,
                isManufactured: false, isMaterial: true, isSalesItem: false, isShippable: false,
                reqStock: p.Stock, reqReorder: p.Stock / 3, reqMin: 10, mfgMinutes: 0));

        // Consumables — purchased, tracked, high stock. (sku, name, category, vendorName, cost, stock)
        var consumables = new (string Sku, string Name, string Category, string Vendor, decimal Cost, int Stock)[]
        {
            ("SCR-M3", "M3 Screw", Cat.Fasteners, "FastenRight Hardware", 0.02m, 5000),
            ("SCR-632", "6-32 Screw", Cat.Fasteners, "FastenRight Hardware", 0.02m, 5000),
            ("STANDOFF", "Motherboard Standoff", Cat.Fasteners, "FastenRight Hardware", 0.05m, 3000),
            ("TPASTE", "Thermal Paste Tube", Cat.Consumable, "FrostByte Cooling", 1.20m, 400),
            ("TPAD", "Thermal Pad", Cat.Consumable, "FrostByte Cooling", 0.40m, 600),
            ("CTIE-100", "Cable Tie (100pk)", Cat.Consumable, "LinkLine Cables & Parts", 2.50m, 300),
            ("ESD-BAG", "Anti-Static Bag", Cat.Consumable, "LinkLine Cables & Parts", 0.15m, 2000),
        };
        foreach (var p in consumables)
            products.Add(MakeProduct(p.Sku, p.Name, Cls.Consumable, p.Category, p.Vendor,
                ourCost: p.Cost, salesPrice: 0m, listPrice: 0m,
                isManufactured: false, isMaterial: true, isSalesItem: false, isShippable: false,
                reqStock: p.Stock, reqReorder: p.Stock / 3, reqMin: 100, mfgMinutes: 0));

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        foreach (var product in _context.Products)
        {
            _productIdBySku[product.identifier1] = product.id;
            _productById[product.id] = product;
        }

        Console.WriteLine($"  Products seeded ({products.Count}).");
    }

    private Product MakeProduct(
        string sku, string name, string productClass, string category, string vendorName,
        decimal ourCost, decimal salesPrice, decimal listPrice,
        bool isManufactured, bool isMaterial, bool isSalesItem, bool isShippable,
        int reqStock, int reqReorder, int reqMin, int mfgMinutes)
    {
        return Stamp(new Product
        {
            vendor_id = _vendorIdByName.TryGetValue(vendorName, out var vid) ? vid : 0,
            product_class = productClass,
            category = category,
            identifier1 = sku,
            product_name = name,
            internal_description = name,
            our_cost = ourCost,
            unit_cost = ourCost,
            sales_price = salesPrice,
            list_price = listPrice,
            required_stock_level = reqStock,
            required_reorder_level = reqReorder,
            required_min_order = reqMin,
            manufacture_time_minutes = mfgMinutes,
            is_manufactured = isManufactured,
            is_taxable = true,
            is_stock = true,       // so InventoryModule.RebuildCounts builds a count row
            is_material = isMaterial,
            is_rental_item = false,
            is_sales_item = isSalesItem,
            is_labor = false,
            is_shippable = isShippable,
            is_retired = false,    // defaults to true — must be false or it's excluded from inventory
        });
    }

    private async Task SeedBomsAsync()
    {
        // parent SKU -> ordered list of (component SKU, quantity)
        var boms = new (string Parent, (string Child, int Qty)[] Lines)[]
        {
            // Sub-assemblies
            ("SUB-MOBO-ASSY", new[] { ("MB-ATX-B650", 1), ("CPU-RYZEN7", 1), ("RAM-16GB", 2), ("SCR-M3", 6), ("TPASTE", 1) }),
            ("SUB-PSU-ASSY", new[] { ("PSU-650", 1), ("CBL-POWER", 1), ("SCR-632", 4) }),
            ("SUB-COOL-ASSY", new[] { ("COOL-AIR", 1), ("FAN-120", 2), ("TPAD", 1), ("SCR-632", 4) }),

            // Finished goods (2-level: reference sub-assemblies + direct components)
            ("PC-NEBULA-01", new[] { ("SUB-MOBO-ASSY", 1), ("SUB-PSU-ASSY", 1), ("SUB-COOL-ASSY", 1), ("GPU-RTX4070", 1), ("SSD-1TB", 1), ("CASE-ATX", 1), ("FAN-120", 2), ("STANDOFF", 9), ("SCR-M3", 8), ("CTIE-100", 1) }),
            ("PC-ORION-01", new[] { ("SUB-MOBO-ASSY", 1), ("SUB-PSU-ASSY", 1), ("SUB-COOL-ASSY", 1), ("RAM-32GB", 2), ("SSD-2TB", 1), ("HDD-4TB", 1), ("CASE-ATX", 1), ("NIC-1G", 1), ("STANDOFF", 9), ("SCR-M3", 8) }),
            ("PC-PULSAR-01", new[] { ("MB-ITX-B660", 1), ("CPU-COREI5", 1), ("RAM-16GB", 1), ("SSD-1TB", 1), ("PSU-450", 1), ("CASE-ITX", 1), ("COOL-AIR", 1), ("TPASTE", 1), ("STANDOFF", 4), ("SCR-M3", 6) }),
            ("PC-TITAN-1U", new[] { ("MB-ATX-X670", 1), ("CPU-COREI7", 1), ("RAM-32GB", 4), ("SSD-2TB", 2), ("SUB-PSU-ASSY", 1), ("CASE-1U", 1), ("FAN-120", 4), ("NIC-1G", 1), ("STANDOFF", 9), ("SCR-M3", 10) }),
            ("PC-VEGA-01", new[] { ("SUB-MOBO-ASSY", 1), ("SUB-PSU-ASSY", 1), ("SUB-COOL-ASSY", 1), ("GPU-RTX4060", 1), ("RAM-32GB", 2), ("SSD-2TB", 1), ("CASE-ATX", 1), ("STANDOFF", 9), ("SCR-M3", 8) }),
        };

        var rows = new List<BOM>();
        foreach (var bom in boms)
        {
            if (!_productIdBySku.TryGetValue(bom.Parent, out var parentId))
                continue;

            var order = 1;
            foreach (var line in bom.Lines)
            {
                if (!_productIdBySku.TryGetValue(line.Child, out var childId))
                    continue;

                rows.Add(Stamp(new BOM
                {
                    parent_product_id = parentId,
                    product_id = childId,
                    quantity = line.Qty,
                    order_number = order++,
                }));
            }
        }

        _context.BOMs.AddRange(rows);
        await _context.SaveChangesAsync();
        Console.WriteLine($"  BOM rows seeded ({rows.Count}).");
    }
}
