using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using KosmosERP.Database.Models;
using KosmosERP.Database.Views;

namespace KosmosERP.Database
{
    public interface IBaseERPContext
    {
        DbSet<ErrorLog> ErrorLogs { get; set; }
        DbSet<GeneralLog> GeneralLogs { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<UserSessionState> UserSessionStates { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<ProductAttribute> ProductAttributes { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<RolePermission> RolePermissions { get; set; }
        DbSet<ModulePermission> ModulePermissions { get; set; }
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
        DbSet<CreditMemoHeader> CreditMemoHeaders { get; set; }
        DbSet<CreditMemoLine> CreditMemoLines { get; set; }
        DbSet<Contact> Contacts { get; set; }
        DbSet<Opportunity> Opportunities { get; set; }
        DbSet<OpportunityLine> OpportunityLines { get; set; }
        DbSet<Lead> Leads { get; set; }
        DbSet<Activity> Activities { get; set; }
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
        DbSet<DocumentUploadObjectCategory> DocumentUploadObjectCategories { get; set; }
        DbSet<DocumentUploadCategory> DocumentUploadCategories { get; set; }
        DbSet<DocumentUploadRevision> DocumentUploadRevisions { get; set; }
        DbSet<DocumentUploadRevisionTag> DocumentUploadRevisionsTags { get; set; }
        DbSet<DocumentUploadObjectTagTemplate> DocumentUploadObjectTags { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<ProductionOrderHeader> ProductionOrderHeaders { get; set; }
        DbSet<ProductionOrderLine> ProductionOrderLines { get; set; }
        DbSet<BOM> BOMs { get; set; }
        DbSet<Inventory> InventoryCounts { get; set; }
        DbSet<Comment> Comments { get; set; }
        DbSet<Payment> Payments { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        DbSet<SubscriptionEntry> SubscriptionEntries { get; set; }
        DbSet<MessageQueue> MessageQueues { get; set; }
        DbSet<Settings> Settings { get; set; }
        DbSet<ChartOfAccount> ChartOfAccounts { get; set; }
        DbSet<JournalEntryHeader> JournalEntryHeaders { get; set; }
        DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        
        DbSet<vw_OrdersReadyForInvoicing> vw_OrdersReadyForInvoicing { get; set; }
        DbSet<vw_OrdersReadyForScheduling> vw_OrdersReadyForScheduling { get; set; }
        DbSet<vw_PartialInvoices> vw_PartialInvoices { get; set; }
        DbSet<vw_ReadyToShip> vw_ReadyToShip { get; set; }
        
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
        DatabaseFacade Database { get; }
        EntityEntry Update(object entity);
    }
}
