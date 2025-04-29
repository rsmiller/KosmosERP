using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Prometheus.Database.Models;

namespace Prometheus.Database
{
    public interface IBaseERPContext
    {
        DbSet<ErrorLog> ErrorLogs { get; set; }
        DbSet<GeneralLog> GeneralLogs { get; set; }
        DbSet<ModulePermission> ModulePermissions { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<UserSessionState> UserSessionStates { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<ProductAttribute> ProductAttributes { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<RolePermission> RolePermissions { get; set; }
        DbSet<Vendor> Vendors { get; set; }
        DbSet<Customer> Customers { get; set; }
        DbSet<CustomerAddress> CustomerAddresses { get; set; }
        DbSet<Address> Addresses { get; set; }
        DbSet<OrderHeader> OrderHeaders { get; set; }
        DbSet<OrderLine> OrderLines { get; set; }
        DbSet<OrderLineAttribute> OrderLineAttributes { get; set; }
        DbSet<APInvoiceHeader> APInvoiceHeaders { get; set; }
        DbSet<APInvoiceLine> APInvoiceLines { get; set; }
        DbSet<ARInvoiceHeader> ARInvoiceHeaders { get; set; }
        DbSet<ARInvoiceLine> ARInvoiceLines { get; set; }
        DbSet<Contact> Contacts { get; set; }
        DbSet<Opportunity> Opportunities { get; set; }
        DbSet<OpportunityLine> OpportunityLines { get; set; }
        DbSet<Lead> Leads { get; set; }
        DbSet<Country> Countries { get; set; }
        DbSet<State> States { get; set; }
        DbSet<KeyValueStore> KeyValueStores { get; set; }
        DbSet<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        DbSet<PurchaseOrderReceiveHeader> PurchaseOrderReceiveHeaders { get; set; }
        DbSet<PurchaseOrderReceiveLine> PurchaseOrderReceiveLines { get; set; }
        DbSet<PurchaseOrderReceiveUpload> PurchaseOrderReceiveUploads { get; set; }
        DbSet<ShipmentHeader> ShipmentHeaders { get; set; }
        DbSet<ShipmentLine> ShipmentLines { get; set; }
        DbSet<DocumentUpload> DocumentUploads { get; set; }
        DbSet<DocumentUploadObject> DocumentUploadObjects { get; set; }
        DbSet<DocumentUploadRevision> DocumentUploadRevisions { get; set; }
        DbSet<DocumentUploadRevisionTag> DocumentUploadRevisionsTags { get; set; }
        DbSet<DocumentUploadObjectTagTemplate> DocumentUploadObjectTags { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<ProductionOrderHeader> ProductionOrderHeaders { get; set; }
        DbSet<ProductionOrderLine> ProductionOrderLines { get; set; }
        DbSet<BOM> BOMs { get; set; }
        DbSet<Inventory> InventoryCounts { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
        DatabaseFacade Database { get; }
        EntityEntry Update(object entity);
    }
}
