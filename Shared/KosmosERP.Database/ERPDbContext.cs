using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Configurations;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using KosmosERP.Database.Views;

namespace KosmosERP.Database;

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

        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                                        d => d.ToDateTime(TimeOnly.MinValue),
                                        d => DateOnly.FromDateTime(d));

        modelBuilder.Entity<Opportunity>().Property(e => e.expected_close).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<OrderHeader>().Property(e => e.order_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<OrderHeader>().Property(e => e.required_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<ARInvoiceHeader>().Property(e => e.invoice_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<ARInvoiceHeader>().Property(e => e.invoice_due_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<ARInvoiceHeader>().Property(e => e.paid_on).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<ProductionOrderHeader>().Property(e => e.planned_start_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<ProductionOrderHeader>().Property(e => e.actual_completed_on).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<ProductionOrderHeader>().Property(e => e.planned_complete_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<Subscription>().Property(e => e.start_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<Subscription>().Property(e => e.next_date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<Subscription>().Property(e => e.end_date).HasConversion(dateOnlyConverter);

        modelBuilder.ApplyConfiguration(new ErrorLogConfiguration());
        modelBuilder.ApplyConfiguration(new GeneralLogConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionStateConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductAttributeConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new ModulePermissionConfiguration());
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
        modelBuilder.ApplyConfiguration(new CreditMemoHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new CreditMemoLineConfiguration());
        modelBuilder.ApplyConfiguration(new ContactConfiguration());
        modelBuilder.ApplyConfiguration(new LeadConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityLineConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadObjectConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadObjectTagTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadTagConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadObjectCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentUploadCategoryConfiguration());
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
        modelBuilder.ApplyConfiguration(new ProductionOrderHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new ProductionOrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new BOMConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionEntryConfiguration());
        modelBuilder.ApplyConfiguration(new MessageQueueConfiguration());
        modelBuilder.ApplyConfiguration(new SettingsConfiguration());
        modelBuilder.ApplyConfiguration(new ChartOfAccountConfiguration());
        modelBuilder.ApplyConfiguration(new JournalEntryHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new JournalEntryLineConfiguration());
        modelBuilder.ApplyConfiguration(new FinancialTransactionConfiguration());

        ///// Views
        modelBuilder.Entity<vw_OrdersReadyForInvoicing>().ToView("vw_OrdersReadyForInvoicing").HasKey(m => m.order_number);
        modelBuilder.Entity<vw_PartialInvoices>().ToView("vw_PartialInvoices").HasKey(m => m.order_number);
        modelBuilder.Entity<vw_OrdersReadyForScheduling>().ToView("vw_OrdersReadyForScheduling").HasNoKey();
        modelBuilder.Entity<vw_ReadyToShip>().ToView("vw_ReadyToShip").HasNoKey();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Uncomment for migrations
        //if (optionsBuilder.IsConfigured == false)
        //{
        //    optionsBuilder.UseMySQL("server=localhost;port=3306;uid=auser;pwd=12345;database=kosmos_erp_new");
        //}
    }

    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<GeneralLog> GeneralLogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserSessionState> UserSessionStates { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<ModulePermission> ModulePermissions { get; set; }
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
    public DbSet<CreditMemoHeader> CreditMemoHeaders { get; set; }
    public DbSet<CreditMemoLine> CreditMemoLines { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<OpportunityLine> OpportunityLines { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Activity> Activities { get; set; }
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
    public DbSet<DocumentUploadObjectCategory> DocumentUploadObjectCategories { get; set; }
    public DbSet<DocumentUploadCategory> DocumentUploadCategories { get; set; }
    public DbSet<DocumentUploadRevision> DocumentUploadRevisions { get; set; }
    public DbSet<DocumentUploadRevisionTag> DocumentUploadRevisionsTags { get; set; }
    public DbSet<DocumentUploadObjectTagTemplate> DocumentUploadObjectTags { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ProductionOrderHeader> ProductionOrderHeaders { get; set; }
    public DbSet<ProductionOrderLine> ProductionOrderLines { get; set; }
    public DbSet<BOM> BOMs { get; set; }
    public DbSet<Inventory> InventoryCounts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionEntry> SubscriptionEntries { get; set; }
    public DbSet<MessageQueue> MessageQueues { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<ChartOfAccount> ChartOfAccounts { get; set; }
    public DbSet<JournalEntryHeader> JournalEntryHeaders { get; set; }
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

    public DbSet<vw_OrdersReadyForInvoicing> vw_OrdersReadyForInvoicing { get; set; }
    public DbSet<vw_PartialInvoices> vw_PartialInvoices { get; set; }
    public DbSet<vw_OrdersReadyForScheduling> vw_OrdersReadyForScheduling { get; set; }
    public DbSet<vw_ReadyToShip> vw_ReadyToShip { get; set; }
}
