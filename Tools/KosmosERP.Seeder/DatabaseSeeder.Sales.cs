using KosmosERP.Database.Models;
using KosmosERP.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Seeder;

/// <summary>
/// Milestone 2 — lookups (pay/ship/terms/production/carrier/GL + opportunity stages),
/// users &amp; roles, customers/addresses/contacts, CRM (leads, opportunities, activities),
/// and sales orders. Orders are attributed to salespeople via created_by so the Top
/// Salespeople report resolves them.
/// </summary>
public partial class DatabaseSeeder
{
    // KeyValueStore keys referenced by transactional rows.
    private static class Kv
    {
        public const string ShipPickup = "shipping_method_pickup";
        public const string ShipCarrier = "shipping_method_carrier";
        public const string ShipDispatch = "shipping_method_dispatch";

        public const string PayNetTerms = "pay_method_net_terms";
        public const string PayCreditCard = "pay_method_credit_card";
        public const string PayCheck = "pay_method_check";
        public const string PayWire = "pay_method_wire";

        public const string TermsNet15 = "payment_terms_net_15";
        public const string TermsNet30 = "payment_terms_net_30";
        public const string TermsNet45 = "payment_terms_net_45";
        public const string TermsNet60 = "payment_terms_net_60";

        public const string ProdSubmitted = "production_order_status_submitted";
        public const string ProdPulled = "production_order_status_pulled";
        public const string ProdWip = "production_order_status_wip";
        public const string ProdQc = "production_order_status_qc";
        public const string ProdComplete = "production_order_status_complete";
        public const string ProdReadyToShip = "production_order_status_ready_to_ship";

        public const string CarrierUps = "carrier_ups";
        public const string CarrierFedex = "carrier_fedex";
        public const string CarrierDhl = "carrier_dhl";

        // Opportunity stage keys keep the repo's original spelling ("opporunity").
        public const string StageProspecting = "opporunity_stage_prospecting";
        public const string StageQualifying = "opporunity_stage_qualifying";
        public const string StageProposal = "opporunity_stage_proposal";
        public const string StageNegotiation = "opporunity_stage_negotiation";
        public const string StageClosedWon = "opporunity_stage_closed_won";
        public const string StageClosedLost = "opporunity_stage_closed_lost";
    }

    private const string OpportunityModuleId = "opportunity";
    private static readonly Random Rng = new(20260921);

    private async Task SeedLookupsAsync()
    {
        void AddKv(string key, string value, string moduleId)
        {
            if (_context.KeyValueStores.Any(k => k.module_id == moduleId && k.key == key))
                return;
            _context.KeyValueStores.Add(Stamp(new KeyValueStore { key = key, value = value, module_id = moduleId }));
        }

        AddKv(Kv.ShipPickup, "Customer Pickup", KeyValueIds.ShippingMethods);
        AddKv(Kv.ShipCarrier, "Common Carrier", KeyValueIds.ShippingMethods);
        AddKv(Kv.ShipDispatch, "Company Dispatch", KeyValueIds.ShippingMethods);

        AddKv(Kv.PayNetTerms, "Net Terms", KeyValueIds.PayMethods);
        AddKv(Kv.PayCreditCard, "Credit Card", KeyValueIds.PayMethods);
        AddKv(Kv.PayCheck, "Check", KeyValueIds.PayMethods);
        AddKv(Kv.PayWire, "Wire Transfer", KeyValueIds.PayMethods);

        AddKv(Kv.TermsNet15, "Net 15", KeyValueIds.PaymentTerms);
        AddKv(Kv.TermsNet30, "Net 30", KeyValueIds.PaymentTerms);
        AddKv(Kv.TermsNet45, "Net 45", KeyValueIds.PaymentTerms);
        AddKv(Kv.TermsNet60, "Net 60", KeyValueIds.PaymentTerms);

        AddKv(Kv.ProdSubmitted, "Submitted", KeyValueIds.ProductionStatuses);
        AddKv(Kv.ProdPulled, "Parts Pulled", KeyValueIds.ProductionStatuses);
        AddKv(Kv.ProdWip, "Work In Progress", KeyValueIds.ProductionStatuses);
        AddKv(Kv.ProdQc, "Quality Check", KeyValueIds.ProductionStatuses);
        AddKv(Kv.ProdComplete, "Complete", KeyValueIds.ProductionStatuses);
        AddKv(Kv.ProdReadyToShip, "Ready To Ship", KeyValueIds.ProductionStatuses);

        // Freight carriers — resolved by key regardless of group.
        AddKv(Kv.CarrierUps, "UPS", KeyValueIds.ShippingMethods);
        AddKv(Kv.CarrierFedex, "FedEx", KeyValueIds.ShippingMethods);
        AddKv(Kv.CarrierDhl, "DHL", KeyValueIds.ShippingMethods);

        // Opportunity stages (display names shown on the Top Opportunities report).
        AddKv(Kv.StageProspecting, "Prospecting", OpportunityModuleId);
        AddKv(Kv.StageQualifying, "Qualifying", OpportunityModuleId);
        AddKv(Kv.StageProposal, "Proposal", OpportunityModuleId);
        AddKv(Kv.StageNegotiation, "Negotiation", OpportunityModuleId);
        AddKv(Kv.StageClosedWon, "Closed Won", OpportunityModuleId);
        AddKv(Kv.StageClosedLost, "Closed Lost", OpportunityModuleId);

        // GL-account keys used by module-based posting (harmless here; we post directly).
        AddKv("gl_account_accounts_receivable", "1100", KeyValueIds.GLAccounts);
        AddKv("gl_account_sales_revenue", "4010", KeyValueIds.GLAccounts);
        AddKv("gl_account_accounts_payable", "2010", KeyValueIds.GLAccounts);
        AddKv("gl_account_purchases", "5010", KeyValueIds.GLAccounts);

        await _context.SaveChangesAsync();
        Console.WriteLine("  Lookups seeded (shipping/pay/terms/production/carriers/stages/GL).");
    }

    private async Task SeedUsersAndRolesAsync()
    {
        // (first, last, externalId, isManagement, isSalesperson)
        var users = new (string First, string Last, string ExternalId, bool Mgmt, bool Sales)[]
        {
            ("System", "Admin", "admin", true, false),
            ("Jordan", "Pike", "ext-jordan", false, true),
            ("Riley", "Vance", "ext-riley", false, true),
            ("Morgan", "Lee", "ext-morgan", true, true),
        };

        var index = 0;
        var addedUsers = new List<User>();
        foreach (var u in users)
        {
            var user = Stamp(new User
            {
                first_name = u.First,
                last_name = u.Last,
                username = u.ExternalId,
                password = "seeded",
                password_salt = "seeded",
                employee_number = $"E{100 + index++}",
                external_id = u.ExternalId,
                is_admin = u.ExternalId == "admin",
                is_management = u.Mgmt,
            });
            _context.Users.Add(user);
            addedUsers.Add(user);
        }

        await _context.SaveChangesAsync();

        // Map only the users we just added (avoids colliding with pre-existing rows in the DB).
        foreach (var user in addedUsers)
            _userIdByExternalId[user.external_id] = user.id;

        foreach (var u in users.Where(x => x.Sales))
            _salespersonExternalIds.Add(u.ExternalId);

        // Minimal admin role + membership.
        var adminRole = Stamp(new Role { name = "Administrators" });
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync();

        _context.RolePermissions.Add(Stamp(new RolePermission
        {
            role_id = adminRole.id, module_id = "*",
            read = true, write = true, edit = true, delete = true, requires_admin = true,
        }));
        if (_userIdByExternalId.TryGetValue("admin", out var adminId))
            _context.UserRoles.Add(Stamp(new UserRole { user_id = adminId, role_id = adminRole.id }));

        await _context.SaveChangesAsync();
        Console.WriteLine($"  Users seeded ({users.Length}) + admin role.");
    }

    private async Task SeedCustomersAsync()
    {
        var terms = new[] { Kv.TermsNet15, Kv.TermsNet30, Kv.TermsNet45, Kv.TermsNet60 };
        var categories = new[] { "Reseller", "OEM", "Retailer", "Enterprise" };
        var names = new[]
        {
            "Apex Systems Integrators", "BlueOrbit Retail", "Cascade Computing", "Datacore Solutions",
            "Everest OEM Assembly", "Frontier Tech Supply", "Granite Peak Systems", "Harbor IT Services",
            "Ionix Enterprises", "Juniper Digital", "Keystone Workstations", "Lumen Data Systems",
        };

        var citySeed = new (string City, string State, string Zip)[]
        {
            ("Dallas", "TX", "75201"), ("Denver", "CO", "80202"), ("Seattle", "WA", "98101"),
            ("Chicago", "IL", "60601"), ("Atlanta", "GA", "30301"), ("Phoenix", "AZ", "85001"),
        };

        var number = DatabaseStartNumbers.Customers;
        var idx = 0;
        foreach (var name in names)
        {
            var loc = citySeed[idx % citySeed.Length];

            var address = Stamp(new Address
            {
                street_address1 = $"{100 + idx * 5} Commerce Blvd",
                city = loc.City, state = loc.State, postal_code = loc.Zip, country = "USA",
            });
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            var customer = Stamp(new Customer
            {
                customer_number = number++,
                customer_name = name,
                phone = $"555-01{(idx + 10):D2}",
                general_email = $"info@{name.Split(' ')[0].ToLower()}.example",
                accounting_email = $"ap@{name.Split(' ')[0].ToLower()}.example",
                category = categories[idx % categories.Length],
                payment_terms = terms[idx % terms.Length],
                is_taxable = true,
                tax_rate = 0.0825m,
            });
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _addressIdByCustomerId[customer.id] = address.id;
            _context.CustomerAddresses.Add(Stamp(new CustomerAddress
            {
                customer_id = customer.id, address_id = address.id, address_type_id = CustomerAddressType.ShipTo,
            }));

            // 1–2 contacts each.
            var contactCount = 1 + (idx % 2);
            for (var c = 0; c < contactCount; c++)
            {
                _context.Contacts.Add(Stamp(new Contact
                {
                    customer_id = customer.id,
                    first_name = c == 0 ? "Alex" : "Sam",
                    last_name = name.Split(' ')[0],
                    title = c == 0 ? "Purchasing Manager" : "IT Director",
                    email = $"buyer{c}@{name.Split(' ')[0].ToLower()}.example",
                    phone = $"555-02{(idx + 10):D2}",
                }));
            }

            idx++;
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  Customers seeded ({names.Length}) + addresses + contacts.");
    }

    private async Task SeedCrmAsync()
    {
        // Read contacts straight from the DB so this reflects contacts created this run (the
        // in-memory map is only populated when SeedCustomers is skipped). Every opportunity needs
        // a valid contact_id — the FK is enforced.
        var contactsByCustomer = _context.Contacts.Where(c => !c.is_deleted).ToList()
            .GroupBy(c => c.customer_id).ToDictionary(g => g.Key, g => g.First().id);
        var customers = AddressableCustomers().Where(c => contactsByCustomer.ContainsKey(c.id)).ToList();
        var userIds = _userIdByExternalId.Values.ToList();
        var finishedIds = _productById.Values.Where(p => p.product_class == Cls.Finished).Select(p => p.id).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Leads (~8). owner_id is a User.id (int).
        var leadStages = new[] { "New", "Working", "Qualified", "Unqualified" };
        for (var i = 0; i < 8; i++)
        {
            _context.Leads.Add(Stamp(new Lead
            {
                first_name = $"Lead{i + 1}",
                last_name = "Prospect",
                company_name = $"Prospect Corp {i + 1}",
                title = "Operations Lead",
                email = $"lead{i + 1}@prospect.example",
                phone = $"555-03{i:D2}",
                lead_stage = leadStages[i % leadStages.Length],
                is_converted = i % 5 == 0,
                owner_id = userIds[i % userIds.Count],
                city = "Austin", state = "TX", zip = "78701", country = "USA",
            }));
        }

        // Opportunities (~10). owner_id is a User.external_id (string); mix of open + closed.
        var stages = new[]
        {
            Kv.StageProspecting, Kv.StageQualifying, Kv.StageProposal, Kv.StageNegotiation,
            Kv.StageProposal, Kv.StageNegotiation, Kv.StageClosedWon, Kv.StageClosedLost,
            Kv.StageProposal, Kv.StageQualifying,
        };
        var winByStage = new Dictionary<string, int>
        {
            [Kv.StageProspecting] = 15, [Kv.StageQualifying] = 30, [Kv.StageProposal] = 55,
            [Kv.StageNegotiation] = 75, [Kv.StageClosedWon] = 100, [Kv.StageClosedLost] = 0,
        };

        for (var i = 0; i < stages.Length; i++)
        {
            var customer = customers[i % customers.Count];
            var stage = stages[i];
            var opp = Stamp(new Opportunity
            {
                opportunity_name = $"{customer.customer_name} — PC fleet refresh",
                customer_id = customer.id,
                contact_id = contactsByCustomer.TryGetValue(customer.id, out var cid) ? cid : 0,
                amount = 5000m + i * 3500m,
                stage = stage,
                win_chance = winByStage[stage],
                expected_close = today.AddDays(15 + i * 10),
                owner_id = _salespersonExternalIds[i % _salespersonExternalIds.Count],
            });
            _context.Opportunities.Add(opp);
            await _context.SaveChangesAsync();

            var lineCount = 1 + (i % 2);
            for (var l = 0; l < lineCount; l++)
            {
                var productId = finishedIds[(i + l) % finishedIds.Count];
                var product = _productById[productId];
                _context.OpportunityLines.Add(Stamp(new OpportunityLine
                {
                    opportunity_id = opp.id,
                    product_id = productId,
                    description = product.product_name,
                    line_number = l + 1,
                    quantity = 2 + l,
                    unit_price = product.sales_price,
                }));
            }
        }

        // Activities (~15) tied to customers/opportunities.
        var opps = _context.Opportunities.ToList();
        var types = new[] { "Call", "Meeting", "Email", "Task", "Note" };
        var statuses = new[] { "Completed", "Planned", "In Progress" };
        for (var i = 0; i < 15; i++)
        {
            var opp = opps[i % opps.Count];
            _context.Activities.Add(Stamp(new Activity
            {
                subject = $"{types[i % types.Length]} with {customers[i % customers.Count].customer_name}",
                description = "Follow-up on PC fleet opportunity.",
                activity_type = types[i % types.Length],
                status = statuses[i % statuses.Length],
                owner_id = userIds[i % userIds.Count],
                start_date = DateTime.UtcNow.AddDays(-i * 3),
                priority = 1 + (i % 4),
                customer_id = opp.customer_id,
                opportunity_id = opp.id,
                related_entity_type = "Opportunity",
            }));
        }

        await _context.SaveChangesAsync();
        Console.WriteLine("  CRM seeded (8 leads, 10 opportunities, 15 activities).");
    }

    /// <summary>
    /// Works around an app schema bug: order_headers.id was created without AUTO_INCREMENT
    /// (order_number is also ValueGeneratedOnAdd, and MySQL allows one auto-increment column per
    /// table, so the migration dropped identity from id). Without this, inserting an order fails
    /// with "Field 'id' doesn't have a default value". Idempotent — a no-op if id is already
    /// AUTO_INCREMENT. The proper fix is in OrderHeaderConfiguration + a new migration.
    /// </summary>
    private async Task EnsureOrderHeaderIdentityAsync()
    {
        // The column is referenced by FKs, so this MODIFY is only permitted with FK checks off —
        // which the seeder already holds off for the whole session (see Program.Main).
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE order_headers MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT");
        }
        catch (Exception ex)
        {
            Console.WriteLine("  WARNING: could not ensure order_headers.id AUTO_INCREMENT: " + ex.Message);
        }
    }

    private async Task SeedSalesOrdersAsync()
    {
        await EnsureOrderHeaderIdentityAsync();

        var customers = AddressableCustomers();
        var finished = _productById.Values.Where(p => p.product_class == Cls.Finished).ToList();
        var shipMethods = new[] { Kv.ShipCarrier, Kv.ShipDispatch, Kv.ShipPickup };
        var payMethods = new[] { Kv.PayNetTerms, Kv.PayCreditCard, Kv.PayWire, Kv.PayCheck };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var orderNumber = DatabaseStartNumbers.Orders;

        for (var i = 0; i < 25; i++)
        {
            var customer = customers[i % customers.Count];
            var salesperson = _salespersonExternalIds[i % _salespersonExternalIds.Count];
            var orderDate = today.AddDays(-(i * 12 + Rng.Next(0, 8)));   // spread across ~10 months
            var addressId = _addressIdByCustomerId[customer.id];

            // status mix: mostly complete, some open, a few canceled.
            var canceled = i % 11 == 10;
            var complete = !canceled && i % 4 != 0;

            var header = StampAs(new OrderHeader
            {
                order_number = orderNumber++,
                customer_id = customer.id,
                ship_to_address_id = addressId,
                billing_address_id = addressId,
                shipping_method = shipMethods[i % shipMethods.Length],
                pay_method = payMethods[i % payMethods.Length],
                order_type = "SO",
                order_date = orderDate,
                required_date = orderDate.AddDays(14),
                tax = 0m,
                shipping_cost = 45m,
                is_complete = complete,
                is_canceled = canceled,
                canceled_reason = canceled ? "Customer canceled" : null,
            }, salesperson);
            _context.OrderHeaders.Add(header);
            await _context.SaveChangesAsync();

            var seeded = new SeededOrder
            {
                Id = header.id, CustomerId = customer.id, OrderDate = orderDate,
                Complete = complete, Canceled = canceled, SalespersonExternalId = salesperson,
            };

            var lineCount = 1 + (i % 3);
            decimal total = 0;
            for (var l = 0; l < lineCount; l++)
            {
                var product = finished[(i + l) % finished.Count];
                var qty = 1 + Rng.Next(0, 4);
                var line = StampAs(new OrderLine
                {
                    order_header_id = header.id,
                    product_id = product.id,
                    line_number = l + 1,
                    line_description = product.product_name,
                    quantity = qty,
                    unit_price = product.sales_price,
                }, salesperson);
                _context.OrderLines.Add(line);
                total += qty * product.sales_price;
                seeded.Lines.Add(new SeededOrderLine { ProductId = product.id, Qty = qty, UnitPrice = product.sales_price });
            }

            await _context.SaveChangesAsync();

            // capture order-line ids for downstream (production/shipment/AR).
            var lineIds = _context.OrderLines.Where(x => x.order_header_id == header.id)
                .OrderBy(x => x.line_number).Select(x => x.id).ToList();
            for (var l = 0; l < seeded.Lines.Count && l < lineIds.Count; l++)
                seeded.Lines[l].Id = lineIds[l];

            header.price = total;
            _context.OrderHeaders.Update(header);
            await _context.SaveChangesAsync();

            _orders.Add(seeded);
        }

        Console.WriteLine($"  Sales orders seeded ({_orders.Count}).");
    }
}
