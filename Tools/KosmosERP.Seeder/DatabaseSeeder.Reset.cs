using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Seeder;

/// <summary>
/// Clears all seeded data so the seeder can be re-run from a clean state (invoked by --reset).
///
/// Preserved on purpose:
///   - The first-run "system" user (username == "system") and its user-role links.
///   - The permission/role/module infrastructure (Roles, RolePermissions, ModulePermissions,
///     Modules) that the system user depends on — the app recreates its own on next boot, and
///     leaving it avoids orphaning the system user's role membership.
///
/// Everything else the seeder produces (business/master/lookup data, and every non-system user)
/// is bulk-deleted. Run with the API stopped.
/// </summary>
public partial class DatabaseSeeder
{
    public async Task ResetAsync()
    {
        Console.WriteLine("Resetting seeded data (preserving the 'system' user)...");

        var systemUserId = _context.Users
            .Where(u => u.username == "system")
            .Select(u => (int?)u.id)
            .FirstOrDefault();

        // Accounting
        await _context.Payments.ExecuteDeleteAsync();
        await _context.ARInvoiceLines.ExecuteDeleteAsync();
        await _context.ARInvoiceHeaders.ExecuteDeleteAsync();
        await _context.APInvoiceLines.ExecuteDeleteAsync();
        await _context.APInvoiceHeaders.ExecuteDeleteAsync();
        await _context.FinancialTransactions.ExecuteDeleteAsync();
        await _context.ChartOfAccounts.ExecuteDeleteAsync();

        // Inventory ledger
        await _context.InventoryCounts.ExecuteDeleteAsync();
        await _context.Transactions.ExecuteDeleteAsync();

        // Fulfillment
        await _context.ShipmentLines.ExecuteDeleteAsync();
        await _context.ShipmentHeaders.ExecuteDeleteAsync();
        await _context.ProductionOrderLines.ExecuteDeleteAsync();
        await _context.ProductionOrderHeaders.ExecuteDeleteAsync();
        await _context.PurchaseOrderReceiveLines.ExecuteDeleteAsync();
        await _context.PurchaseOrderReceiveHeaders.ExecuteDeleteAsync();
        await _context.PurchaseOrderLines.ExecuteDeleteAsync();
        await _context.PurchaseOrderHeaders.ExecuteDeleteAsync();

        // Sales
        await _context.OrderLines.ExecuteDeleteAsync();
        await _context.OrderHeaders.ExecuteDeleteAsync();

        // CRM
        await _context.Activities.ExecuteDeleteAsync();
        await _context.OpportunityLines.ExecuteDeleteAsync();
        await _context.Opportunities.ExecuteDeleteAsync();
        await _context.Leads.ExecuteDeleteAsync();

        // Master data
        await _context.Contacts.ExecuteDeleteAsync();
        await _context.CustomerAddresses.ExecuteDeleteAsync();
        await _context.Customers.ExecuteDeleteAsync();
        await _context.BOMs.ExecuteDeleteAsync();
        await _context.Products.ExecuteDeleteAsync();
        await _context.Vendors.ExecuteDeleteAsync();
        await _context.Addresses.ExecuteDeleteAsync();

        // Lookups / config
        await _context.KeyValueStores.ExecuteDeleteAsync();
        await _context.Settings.ExecuteDeleteAsync();

        // Security — keep the system user and (if present) its role links.
        if (systemUserId.HasValue)
        {
            await _context.UserRoles.Where(ur => ur.user_id != systemUserId.Value).ExecuteDeleteAsync();
            await _context.Users.Where(u => u.username != "system").ExecuteDeleteAsync();
        }
        else
        {
            await _context.UserRoles.ExecuteDeleteAsync();
            await _context.Users.ExecuteDeleteAsync();
        }

        Console.WriteLine(systemUserId.HasValue
            ? "Reset complete. Preserved the 'system' user (id " + systemUserId.Value + ")."
            : "Reset complete. No 'system' user was found to preserve.");
    }
}
