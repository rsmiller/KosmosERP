using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.Seeder;

/// <summary>
/// Milestone 3 — fulfillment (purchase orders + receipts, production orders, shipments) and the
/// inventory ledger. Production orders emit no inventory movements, so this tier writes the
/// <c>Transaction</c> rows directly (purchased/received/reserved/shipped + finished-good production)
/// and then materializes the stored <c>inventory</c> rows using InventoryModule.RebuildCounts' formula.
/// </summary>
public partial class DatabaseSeeder
{
    // Shipment lines captured for the inventory ledger (order_line_id -> shipped qty).
    private readonly List<(int OrderLineId, int Qty)> _shipmentLines = new();

    private async Task SeedPurchaseOrdersAsync()
    {
        // Components/consumables grouped by their supplying vendor.
        var vendorComponents = _productById.Values
            .Where(p => p.product_class == Cls.Component || p.product_class == Cls.Consumable)
            .GroupBy(p => p.vendor_id)
            .ToDictionary(g => g.Key, g => g.ToList());

        var vendorIds = vendorComponents.Keys.ToList();
        var poNumber = 5000;

        for (var i = 0; i < 15; i++)
        {
            var vendorId = vendorIds[i % vendorIds.Count];
            var supplyable = vendorComponents[vendorId];

            // receiving pattern: open (none) / partial / full
            var mode = i % 3; // 0 = open, 1 = partial, 2 = full
            var complete = mode == 2;

            var header = Stamp(new PurchaseOrderHeader
            {
                vendor_id = vendorId,
                po_type = "PO",
                po_number = poNumber++,
                tax = 0m,
                is_complete = complete,
                is_canceled = false,
                completed_on = complete ? DateTime.UtcNow.AddDays(-i * 15 + 5) : null,
            });
            header.created_on = DateTime.UtcNow.AddDays(-i * 15); // spread for "Recent POs"
            _context.PurchaseOrderHeaders.Add(header);
            await _context.SaveChangesAsync();

            var po = new SeededPO { Id = header.id, VendorId = vendorId, Complete = complete, CreatedOn = header.created_on };

            var lineCount = 1 + (i % 3);
            decimal total = 0;
            for (var l = 0; l < lineCount; l++)
            {
                var product = supplyable[(i + l) % supplyable.Count];
                var qty = 50 + Rng.Next(0, 150);
                var received = mode == 0 ? 0 : mode == 1 ? (int)(qty * 0.6) : qty;

                var line = Stamp(new PurchaseOrderLine
                {
                    purchase_order_header_id = header.id,
                    product_id = product.id,
                    line_number = l + 1,
                    quantity = qty,
                    description = product.product_name,
                    unit_price = product.our_cost,
                    is_complete = mode == 2,
                });
                _context.PurchaseOrderLines.Add(line);
                await _context.SaveChangesAsync();

                total += qty * product.our_cost;
                po.Lines.Add(new SeededPOLine { Id = line.id, ProductId = product.id, Qty = qty, UnitPrice = product.our_cost, ReceivedQty = received });
            }

            header.price = total;
            _context.PurchaseOrderHeaders.Update(header);
            await _context.SaveChangesAsync();

            // Receipts for partial/full POs.
            if (mode != 0)
            {
                var receiveHeader = Stamp(new PurchaseOrderReceiveHeader
                {
                    purchase_order_id = header.id,
                    units_ordered = po.Lines.Sum(x => x.Qty),
                    units_received = po.Lines.Sum(x => x.ReceivedQty),
                    is_complete = complete,
                    completed_on = complete ? DateTime.UtcNow.AddDays(-i * 15 + 5) : null,
                });
                _context.PurchaseOrderReceiveHeaders.Add(receiveHeader);
                await _context.SaveChangesAsync();

                foreach (var line in po.Lines)
                {
                    _context.PurchaseOrderReceiveLines.Add(Stamp(new PurchaseOrderReceiveLine
                    {
                        purchase_order_receive_header_id = receiveHeader.id,
                        purchase_order_line_id = line.Id,
                        units_ordered = line.Qty,
                        units_received = line.ReceivedQty,
                        is_complete = line.ReceivedQty >= line.Qty,
                    }));
                }
                await _context.SaveChangesAsync();
            }

            _purchaseOrders.Add(po);
        }

        Console.WriteLine($"  Purchase orders seeded ({_purchaseOrders.Count}) with receipts.");
    }

    private async Task SeedProductionOrdersAsync()
    {
        var statuses = new[]
        {
            Kv.ProdSubmitted, Kv.ProdPulled, Kv.ProdWip, Kv.ProdQc, Kv.ProdReadyToShip, Kv.ProdComplete,
        };

        var eligible = _orders.Where(o => !o.Canceled).Take(14).ToList();
        var count = 0;

        for (var i = 0; i < eligible.Count; i++)
        {
            var order = eligible[i];
            var status = statuses[i % statuses.Length];
            var isComplete = status == Kv.ProdComplete;

            var header = Stamp(new ProductionOrderHeader
            {
                order_header_id = order.Id,
                status = status,
                priority_id = 1 + (i % 3),
                planned_start_date = order.OrderDate.AddDays(1),
                planned_complete_date = order.OrderDate.AddDays(7),
                actual_completed_on = isComplete ? order.OrderDate.AddDays(6) : null,
                is_complete = isComplete,
                production_lead_minutes = 240,
            });
            _context.ProductionOrderHeaders.Add(header);
            await _context.SaveChangesAsync();

            var lineNo = 1;
            foreach (var line in order.Lines)
            {
                _context.ProductionOrderLines.Add(Stamp(new ProductionOrderLine
                {
                    production_order_header_id = header.id,
                    order_line_id = line.Id,
                    line_number = lineNo++,
                    quantity = line.Qty,
                    production_lead_minutes = 120,
                    status = status,
                    is_complete = isComplete,
                    completed_on = isComplete ? DateTime.UtcNow.AddDays(-1) : null,
                }));
            }
            await _context.SaveChangesAsync();
            count++;
        }

        Console.WriteLine($"  Production orders seeded ({count}).");
    }

    private async Task SeedShipmentsAsync()
    {
        var carriers = new[] { Kv.CarrierUps, Kv.CarrierFedex, Kv.CarrierDhl };
        var shipmentNumber = DatabaseStartNumbers.Shipments;
        var count = 0;

        // Ship most non-canceled orders; complete orders fully, some others partially.
        var shippable = _orders.Where(o => !o.Canceled).ToList();

        for (var i = 0; i < shippable.Count; i++)
        {
            var order = shippable[i];

            // Skip a few so some orders remain fully unshipped (Products Awaiting Shipment).
            if (i % 5 == 4)
                continue;

            var fully = order.Complete && i % 3 != 1; // completed orders usually ship in full
            var addressId = _addressIdByCustomerId[order.CustomerId];

            var header = Stamp(new ShipmentHeader
            {
                order_header_id = order.Id,
                shipment_number = shipmentNumber++,
                address_id = addressId,
                units_to_ship = order.Lines.Sum(x => x.Qty),
                units_shipped = 0,
                is_released = true,
                is_complete = fully,
                ship_via = Kv.ShipCarrier,
                freight_carrier = carriers[i % carriers.Length],
                freight_charge_amount = 35m,
                completed_on = fully ? DateTime.UtcNow.AddDays(-(i * 10)) : null,
            });
            _context.ShipmentHeaders.Add(header);
            await _context.SaveChangesAsync();

            var totalShipped = 0;
            foreach (var line in order.Lines)
            {
                var shipQty = fully ? line.Qty : (int)Math.Ceiling(line.Qty / 2.0); // partial
                totalShipped += shipQty;

                _context.ShipmentLines.Add(Stamp(new ShipmentLine
                {
                    shipment_header_id = header.id,
                    order_line_id = line.Id,
                    units_to_ship = line.Qty,
                    units_shipped = shipQty,
                    is_complete = shipQty >= line.Qty,
                }));

                _shipmentLines.Add((line.Id, shipQty));
            }

            header.units_shipped = totalShipped;
            _context.ShipmentHeaders.Update(header);
            await _context.SaveChangesAsync();
            count++;
        }

        Console.WriteLine($"  Shipments seeded ({count}).");
    }

    private async Task SeedInventoryAsync()
    {
        var now = DateTime.UtcNow;
        var productIdByOrderLineId = _orders.SelectMany(o => o.Lines).ToDictionary(l => l.Id, l => l.ProductId);

        void AddTxn(int productId, int type, int refId, int sold = 0, int shipped = 0, int purchased = 0, int received = 0, decimal cost = 0, decimal price = 0)
        {
            _context.Transactions.Add(Stamp(new Transaction
            {
                product_id = productId,
                transaction_type = type,
                transaction_date = now,
                object_reference_id = refId,
                units_sold = sold,
                units_shipped = shipped,
                units_purchased = purchased,
                units_received = received,
                purchased_unit_cost = cost,
                sold_unit_price = price,
            }));
        }

        // 1) Opening stock for materials (components, consumables, sub-assemblies).
        foreach (var p in _productById.Values.Where(p => p.is_material))
            AddTxn(p.id, TransactionType.Adjustment, 0, received: p.required_stock_level, cost: p.our_cost);

        // 2) Purchase orders: purchased (on-order) for every line; received for receipts.
        foreach (var po in _purchaseOrders)
            foreach (var line in po.Lines)
            {
                AddTxn(line.ProductId, TransactionType.Planned, po.Id, purchased: line.Qty, cost: line.UnitPrice);
                if (line.ReceivedQty > 0)
                    AddTxn(line.ProductId, TransactionType.Inbound, po.Id, received: line.ReceivedQty, cost: line.UnitPrice);
            }

        // 3) Finished-good production: produce enough to cover sales demand + a buffer.
        var demandByProduct = _orders.Where(o => !o.Canceled).SelectMany(o => o.Lines)
            .GroupBy(l => l.ProductId).ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));
        foreach (var p in _productById.Values.Where(p => p.product_class == Cls.Finished))
        {
            var demand = demandByProduct.TryGetValue(p.id, out var d) ? d : 0;
            var produced = demand + p.required_stock_level;
            if (produced > 0)
                AddTxn(p.id, TransactionType.Inbound, 0, received: produced, cost: p.our_cost);
        }

        // 4) Sales orders: reserved (units_sold) for finished goods on non-canceled orders.
        foreach (var order in _orders.Where(o => !o.Canceled))
            foreach (var line in order.Lines)
                AddTxn(line.ProductId, TransactionType.Reserved, order.Id, sold: line.Qty, price: line.UnitPrice);

        // 5) Shipments: outbound (units_shipped).
        foreach (var (orderLineId, qty) in _shipmentLines)
            if (productIdByOrderLineId.TryGetValue(orderLineId, out var productId))
                AddTxn(productId, TransactionType.Outbound, 0, shipped: qty);

        await _context.SaveChangesAsync();

        // 6) Materialize inventory counts (RebuildCounts formula) for every stock product.
        var stockProducts = _productById.Values
            .Where(p => !p.is_retired && (p.is_stock || p.is_material || p.is_rental_item))
            .ToList();

        foreach (var prod in stockProducts)
        {
            var soldSum = _context.Transactions.Where(t => t.product_id == prod.id).Sum(t => t.units_sold);
            var receivedSum = _context.Transactions.Where(t => t.product_id == prod.id).Sum(t => t.units_received);
            var shippedSum = _context.Transactions.Where(t => t.product_id == prod.id).Sum(t => t.units_shipped);
            var purchasedSum = _context.Transactions.Where(t => t.product_id == prod.id).Sum(t => t.units_purchased);

            var available = receivedSum - soldSum;
            var reserved = soldSum - shippedSum;
            var onOrder = purchasedSum - receivedSum;
            var currentStock = available - reserved;
            var toOrder = prod.required_stock_level - (currentStock + onOrder);

            _context.InventoryCounts.Add(new Inventory
            {
                product_id = prod.id,
                product_name = prod.product_name,
                on_hand = available,
                reserved = reserved,
                on_order = onOrder,
                current_stock = currentStock,
                to_order = toOrder < 0 ? 0 : toOrder,
                required_stock = prod.required_stock_level,
                reorder_level = prod.required_reorder_level,
                total_units_sold = soldSum,
                total_units_received = receivedSum,
                total_units_shipped = shippedSum,
                total_on_purchased = purchasedSum,
                created_on = now,
                updated_on = now,
            });
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  Inventory ledger seeded ({_context.Transactions.Count()} transactions, {stockProducts.Count} counts).");
    }
}
