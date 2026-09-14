using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public interface IInventoryCalculatorJob
{
    Task Run();
}

public class InventoryCalculatorJob : IInventoryCalculatorJob
{
    private IBaseERPContext _Context;
    
    private readonly ILogger<BillingPaymentEntryJob> _logger;

    public InventoryCalculatorJob(ILogger<BillingPaymentEntryJob> logger, IBaseERPContext context)
    {
        _logger = logger;

        _Context = context;
    }

    public async Task Run()
    {
        _logger.LogInformation("Running...");

        try
        {
            var all_products = await _Context.Products.Select(m => new { m.id, m.required_stock_level, m.product_name }).ToListAsync();

            foreach(var product in all_products)
            {
                var total_units_sold = await _Context.Transactions.Where(m => m.product_id == product.id).SumAsync(m => m.units_sold);
                var total_units_received = await _Context.Transactions.Where(m => m.product_id == product.id).SumAsync(m => m.units_received);
                var total_units_shipped = await _Context.Transactions.Where(m => m.product_id == product.id).SumAsync(m => m.units_shipped);
                var total_on_purchased = await _Context.Transactions.Where(m => m.product_id == product.id).SumAsync(m => m.units_purchased);

                var units_available = total_units_received - total_units_sold;
                var units_reserved = total_units_sold - total_units_shipped;
                var units_on_order = total_on_purchased - total_units_received;

                var existing_inventory_item = await _Context.InventoryCounts.Where(m => m.product_id == product.id).SingleOrDefaultAsync();
                if(existing_inventory_item == null)
                {
                    // Create
                    await _Context.InventoryCounts.AddAsync(new Inventory()
                    {
                        product_id = product.id,
                        product_name = product.product_name,
                        current_stock = product.required_stock_level,
                        on_hand = units_available,
                        on_order = units_on_order,
                        reserved = units_reserved,
                    });

                    await _Context.SaveChangesAsync();
                }
                else
                {
                    // Update
                    existing_inventory_item.current_stock = product.required_stock_level;
                    existing_inventory_item.on_hand = units_available;
                    existing_inventory_item.on_order = units_on_order;
                    existing_inventory_item.reserved = units_reserved;

                    _Context.Update(existing_inventory_item);
                    await _Context.SaveChangesAsync();
                }
            }
        }
        catch(Exception e)
        {
           _logger.LogError("InventoryCalculatorJob general error; {1}", e.Message); 
        }
    }
}