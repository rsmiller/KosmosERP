using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Configurations;
using Prometheus.Database.Models;

namespace Prometheus.Database;

public partial class ERPDbContext : DbContext, IERPDatabaseContext, IBaseERPContext
{
    public ERPDbContext()
    {
    }

    public ERPDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ErrorLogConfiguration());
        modelBuilder.ApplyConfiguration(new GeneralLogConfiguration());
        modelBuilder.ApplyConfiguration(new ModulePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionStateConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductAttributeConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new VendorConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerAddressConfiguration());
        modelBuilder.ApplyConfiguration(new OrderHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new OrderLineAttributeConfiguration());
        modelBuilder.ApplyConfiguration(new APInvoiceHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new APInvoiceLineConfiguration());
        modelBuilder.ApplyConfiguration(new ARInvoiceHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new ARInvoiceLineConfiguration());
        modelBuilder.ApplyConfiguration(new ContactConfiguration());
        modelBuilder.ApplyConfiguration(new LeadConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityLineConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadObjectConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadObjectTagTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadTagConfiguration());
        modelBuilder.ApplyConfiguration(new KeyValueStoreConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseOrderHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseOrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseReceiveHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseReceiveLineConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseReceiveUploadConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentLineConfiguration());
        modelBuilder.ApplyConfiguration(new StateConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }
    

    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<GeneralLog> GeneralLogs { get; set; }
    public DbSet<ModulePermission> ModulePermissions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserSessionState> UserSessionStates { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<OrderHeader> OrderHeaders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<OrderLineAttribute> OrderLineAttributes { get; set; }
    public DbSet<APInvoiceHeader> APInvoiceHeaders { get; set; }
    public DbSet<APInvoiceLine> APInvoiceLines { get; set; }
    public DbSet<ARInvoiceHeader> ARInvoiceHeaders { get; set; }
    public DbSet<ARInvoiceLine> ARInvoiceLines { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<OpportunityLine> OpportunityLines { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<State> States { get; set; }
    public DbSet<KeyValueStore> KeyValueStores { get; set; }
    public DbSet<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    public DbSet<PurchaseOrderReceiveHeader> PurchaseOrderReceiveHeaders { get; set; }
    public DbSet<PurchaseOrderReceiveLine> PurchaseOrderReceiveLines { get; set; }
    public DbSet<PurchaseOrderReceiveUpload> PurchaseOrderReceiveUploads { get; set; }
    public DbSet<ShipmentHeader> ShipmentHeaders { get; set; }
    public DbSet<ShipmentLine> ShipmentLines { get; set; }
    public DbSet<DocumentUpload> DocumentUploads { get; set; }
    public DbSet<DocumentUploadObject> DocumentUploadObjects { get; set; }
    public DbSet<DocumentUploadRevision> DocumentUploadRevisions { get; set; }
    public DbSet<DocumentUploadRevisionTag> DocumentUploadRevisionsTags { get; set; }
    public DbSet<DocumentUploadObjectTagTemplate> DocumentUploadObjectTags { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
}
