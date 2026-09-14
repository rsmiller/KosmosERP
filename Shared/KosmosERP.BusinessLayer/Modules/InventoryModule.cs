using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Inventory.Dto;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.BusinessLayer.Modules;

public interface IInventoryModule
{
    Task<Response<List<InventoryDto>>> GetCounts();
    Task<Response<bool>> RebuildCounts();
    Task<InventoryDto> MapToDto(Inventory databaseModel);

}

public class InventoryModule : BaseERPModule, IInventoryModule
{
    public override Guid ModuleIdentifier => Guid.Parse("0ded003d-e411-48d9-a5e7-7102e42f36a0");
    public override string ModuleName => "Inventory";

    private IBaseERPContext _Context;

    public InventoryModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Inventory Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Inventory Administrators",
            }, 1));

            _Context.SaveChanges();
        }

        // Seed ModulePermissions for Order module
        var existing_permissions = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString());
        if (!existing_permissions)
        {
            _Context.ModulePermissions.AddRange(new[]
            {
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Read Inventory",
                    internal_permission_name = "read_inventory",
                    read = true,
                    write = false,
                    edit = false,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Create Inventory",
                    internal_permission_name = "create_inventory",
                    read = false,
                    write = true,
                    edit = false,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Edit Inventory",
                    internal_permission_name = "edit_inventory",
                    read = false,
                    write = false,
                    edit = true,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Delete Inventory",
                    internal_permission_name = "delete_inventory",
                    read = false,
                    write = false,
                    edit = false,
                    delete = true,
                    is_active = true
                }, 1)
            });

            _Context.SaveChanges();
        }
    }

    public async Task<Response<List<InventoryDto>>> GetCounts()
    {
        Response<List<InventoryDto>> response = new Response<List<InventoryDto>>();
        response.Data = new List<InventoryDto>();

        var inventory_items = await _Context.InventoryCounts.ToListAsync();
        foreach (var item in inventory_items)
            response.Data.Add(await this.MapToDto(item));

        return response;
    }

    public async Task<Response<bool>> RebuildCounts()
    {
        var products = await _Context.Products.Where(m => !m.is_deleted && !m.is_retired && (m.is_stock || m.is_material || m.is_rental_item)).ToListAsync();
        var inventory_items = await _Context.InventoryCounts.ToListAsync();

        foreach (var prod in products)
        {
            var found_item = inventory_items.SingleOrDefault(m => m.product_id == prod.id);

            // Even though there is a possability that these calls will not be used in a delete, it is likily that only a few will be deleted between run times
            var total_units_sold = await _Context.Transactions.Where(m => m.product_id == prod.id).SumAsync(m => m.units_sold);
            var total_units_received = await _Context.Transactions.Where(m => m.product_id == prod.id).SumAsync(m => m.units_received);
            var total_units_shipped = await _Context.Transactions.Where(m => m.product_id == prod.id).SumAsync(m => m.units_shipped);
            var total_on_purchased = await _Context.Transactions.Where(m => m.product_id == prod.id).SumAsync(m => m.units_purchased);

            var units_available = total_units_received - total_units_sold;
            var units_reserved = total_units_sold - total_units_shipped;
            var units_on_order = total_on_purchased - total_units_received;
            var current_stock = units_available - units_reserved;
            var to_order = prod.required_stock_level - (current_stock + units_on_order);
            
            if (found_item != null)
            {
                if (prod.is_deleted || prod.is_retired)
                {
                    // Delete it

                    _Context.InventoryCounts.Remove(found_item);
                    await _Context.SaveChangesAsync();
                }
                else
                {
                    // Update
                    found_item.product_name = prod.product_name;
                    found_item.on_hand = units_available;
                    found_item.on_order = units_on_order;
                    found_item.current_stock = current_stock;
                    found_item.reserved = units_reserved;
                    found_item.to_order = to_order < 0 ? 0 : to_order;

                    found_item.required_stock = prod.required_stock_level;
                    found_item.reorder_level = prod.required_reorder_level;

                    found_item.total_units_sold = total_units_sold;
                    found_item.total_units_received = total_units_received;
                    found_item.total_units_shipped = total_units_shipped;
                    found_item.total_on_purchased = total_on_purchased;

                    found_item.updated_on = DateTime.UtcNow;

                    _Context.InventoryCounts.Update(found_item);
                    await _Context.SaveChangesAsync();
                }

            }
            else
            {
                // Add
                var new_inventory = this.MapToDatabase(prod);

                new_inventory.product_name = prod.product_name;
                new_inventory.on_hand = units_available;
                new_inventory.on_order = units_on_order;
                new_inventory.current_stock = current_stock;
                new_inventory.reserved = units_reserved;
                new_inventory.to_order = to_order < 0 ? 0 : to_order;

                new_inventory.total_units_sold = total_units_sold;
                new_inventory.total_units_received = total_units_received;
                new_inventory.total_units_shipped = total_units_shipped;
                new_inventory.total_on_purchased = total_on_purchased;

                new_inventory.required_stock = prod.required_stock_level;
                new_inventory.reorder_level = prod.required_reorder_level;

                await _Context.InventoryCounts.AddAsync(new_inventory);
                await _Context.SaveChangesAsync();
            }
        }

        return new Response<bool>(true);
    }

    private Inventory MapToDatabase(Product databaseModel)
    {
        var now = DateTime.UtcNow;

        var model = new Inventory()
        {
            product_id = databaseModel.id,
            required_stock = databaseModel.required_stock_level,
            reorder_level = databaseModel.required_reorder_level,
            on_hand = 0,
            reserved = 0,
            on_order = 0,
            guid = databaseModel.guid,
            created_on = now,
            updated_on = now,
        };

        return model;
    }

    public async Task<InventoryDto> MapToDto(Inventory databaseModel)
    {
        var dto = new InventoryDto()
        {
            id = databaseModel.id,
            product_id = databaseModel.product_id,
            product_name = databaseModel.product_name,
            current_stock = databaseModel.current_stock,
            required_stock = databaseModel.required_stock,
            reorder_level = databaseModel.reorder_level,
            on_hand = databaseModel.on_hand,
            reserved = databaseModel.reserved,
            on_order = databaseModel.on_order,
            to_order = databaseModel.to_order,
            total_units_received = databaseModel.total_units_received,
            total_units_shipped = databaseModel.total_units_shipped,
            total_units_sold = databaseModel.total_units_sold,
            total_on_purchased = databaseModel.total_units_sold,
            guid = databaseModel.guid,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
        };

        return dto;
    }
}