using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KosmosERP.BusinessLayer
{
    public static class DependencyInjection
    {
        public static void AddMemoryServices(this IServiceCollection services)
        {
            services.AddScoped<IMemoryCacheService<KeyValueStore>, MemoryCacheService<KeyValueStore>>();
            services.AddScoped<IMemoryCacheService<Customer>, MemoryCacheService<Customer>>();
            services.AddScoped<IMemoryCacheService<OrderHeader>, MemoryCacheService<OrderHeader>>();
            services.AddScoped<IMemoryCacheService<Product>, MemoryCacheService<Product>>();
        }
        public static void AddModules(this IServiceCollection services)
        {
            services.AddScoped<ITokenModule, TokenModule>();

            services.AddScoped<IUserModule, UserModule>();
            services.AddScoped<IAddressModule, AddressModule>();
            services.AddScoped<IStateModule, StateModule>();
            services.AddScoped<ICountryModule, CountryModule>();

            services.AddScoped<ICustomerModule, CustomerModule>();
            services.AddScoped<IOpportunityModule, OpportunityModule>();
            services.AddScoped<ILeadModule, LeadModule>();
            services.AddScoped<IContactModule, ContactModule>();
            services.AddScoped<IActivityModule, ActivityModule>();

            services.AddScoped<IShipmentModule, ShipmentModule>();

            services.AddScoped<IAPInvoiceModule, APInvoiceModule>();
            services.AddScoped<IARInvoiceModule, ARInvoiceModule>();
            services.AddScoped<ICreditMemoModule, CreditMemoModule>();
            services.AddScoped<IPaymentModule, PaymentModule>();
            services.AddScoped<ISubscriptionModule, SubscriptionModule>();

            services.AddScoped<IChartOfAccountModule, ChartOfAccountModule>();
            services.AddScoped<IJournalEntryModule, JournalEntryModule>();
            services.AddScoped<IFinancialTransactionModule, FinancialTransactionModule>();

            services.AddScoped<IOrderModule, OrderModule>();
            services.AddScoped<IPurchaseOrderModule, PurchaseOrderModule>();
            services.AddScoped<IPurchaseOrderReceiveModule, PurchaseOrderReceiveModule>();

            services.AddScoped<IBOMModule, BOMModule>();
            services.AddScoped<IProductionOrderModule, ProductionOrderModule>();

            services.AddScoped<IDocumentUploadModule, DocumentUploadModule>();

            services.AddScoped<IProductModule, ProductModule>();
            services.AddScoped<IVendorModule, VendorModule>();
            services.AddScoped<ITransactionModule, TransactionModule>();
            services.AddScoped<IInventoryModule, InventoryModule>();

            services.AddScoped<INotificationModule, NotificationModule>();
            services.AddScoped<IKeyValueModule, KeyValueModule>();
            services.AddScoped<ICommentModule, CommentModule>();

            services.AddScoped<IGlobalSearchModule, GlobalSearchModule>();
            services.AddScoped<ISettingsModule, SettingsModule>();

            services.AddScoped<ISAMLModule, SAMLModule>();
        }
    }
}
