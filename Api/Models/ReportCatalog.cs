namespace KosmosERP.Api.Models;

/// <summary>
/// A single tunable input for a report (mapped to a query-string parameter on the
/// report endpoint). The UI renders an input control based on <see cref="Type"/> and
/// only sends values the user actually fills in.
/// </summary>
public sealed class ReportParameterDto
{
    /// <summary>Query-string key expected by the endpoint (e.g. "as_of_date").</summary>
    public required string Name { get; init; }

    /// <summary>Human-readable label for the input control.</summary>
    public required string Label { get; init; }

    /// <summary>Control hint: "date", "int", "string", or "select".</summary>
    public required string Type { get; init; }

    /// <summary>Whether the endpoint needs this value (most general reports default server-side).</summary>
    public bool Required { get; init; }

    /// <summary>
    /// When set, the parameter is a dropdown: the UI fetches its options from this endpoint
    /// (path relative to the API root, no leading slash) instead of showing a free-text input.
    /// Used together with <see cref="Type"/> = "select".
    /// </summary>
    public string? OptionsEndpoint { get; init; }
}

/// <summary>
/// One selectable option for a dropdown report parameter. <see cref="Value"/> is what the
/// report endpoint expects for the parameter; <see cref="Label"/> is shown to the user.
/// </summary>
public sealed class ReportOptionDto
{
    public required string Value { get; init; }
    public required string Label { get; init; }
}

/// <summary>
/// Describes one general report the UI can request: what to call it, which endpoint
/// serves it, and which parameters it accepts. General reports summarize across records
/// (aging, valuations, sales) rather than rendering a single document (e.g. one packing slip).
/// </summary>
public sealed class ReportCatalogItemDto
{
    /// <summary>Stable report key matching <c>IReportGenerator.ReportKey</c> (e.g. "ar_aging").</summary>
    public required string Key { get; init; }

    /// <summary>Display name shown in the sidebar and viewer header.</summary>
    public required string Name { get; init; }

    /// <summary>One-line description of what the report shows.</summary>
    public required string Description { get; init; }

    /// <summary>Owning category name (matches the parent <see cref="ReportCategoryDto.Name"/>).</summary>
    public required string Category { get; init; }

    /// <summary>
    /// Endpoint path relative to the API root without a leading slash, e.g. "api/v1/Reports/ArAging".
    /// The UI appends parameters and a <c>format</c> query value to fetch the rendered report.
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>Parameters the report accepts. Empty when the report takes no inputs.</summary>
    public List<ReportParameterDto> Parameters { get; init; } = new();
}

/// <summary>A named grouping of general reports, used to build the reports-page sidebar.</summary>
public sealed class ReportCategoryDto
{
    public required string Name { get; init; }
    public List<ReportCatalogItemDto> Reports { get; init; } = new();
}

/// <summary>
/// The catalog of "general" reports surfaced on the Reports page. These are the
/// cross-record reports that stand on their own — deliberately excluding document-style
/// reports that require a specific entity id (AR invoice, packing slip, purchase order,
/// sales order acknowledgement, production traveler, BOM), which are served from their
/// own document screens instead. Endpoints and parameters mirror
/// <see cref="Controllers.ReportsController"/>; keep the two in sync when adding reports.
/// </summary>
public static class ReportCatalog
{
    private static readonly List<ReportParameterDto> AsOfDateOnly = new()
    {
        new() { Name = "as_of_date", Label = "As of date", Type = "date", Required = false },
    };

    private static readonly List<ReportParameterDto> DateRange = new()
    {
        new() { Name = "date_from", Label = "From date", Type = "date", Required = false },
        new() { Name = "date_to", Label = "To date", Type = "date", Required = false },
    };

    // Product-category dropdown, shared by every report that filters on Product.category.
    // Options come from the report catalog's ProductCategories endpoint; the submitted value
    // is the stored category key that the reports filter on.
    private static readonly ReportParameterDto ProductCategorySelect = new()
    {
        Name = "category", Label = "Category", Type = "select", Required = false,
        OptionsEndpoint = "api/v1/Reports/ProductCategories",
    };

    public static readonly List<ReportCategoryDto> General = new()
    {
        new ReportCategoryDto
        {
            Name = "Accounting",
            Reports = new()
            {
                new()
                {
                    Key = "ar_aging", Name = "AR Aging", Category = "Accounting",
                    Description = "Open receivable balances per customer, bucketed by days past due.",
                    Endpoint = "api/v1/Reports/ArAging",
                    Parameters = AsOfDateOnly,
                },
                new()
                {
                    Key = "ap_aging", Name = "AP Aging", Category = "Accounting",
                    Description = "Open payable balances per vendor, bucketed by days past due.",
                    Endpoint = "api/v1/Reports/ApAging",
                    Parameters = AsOfDateOnly,
                },
                new()
                {
                    Key = "income_statement", Name = "Income Statement", Category = "Accounting",
                    Description = "Profit & loss over a date range (defaults to year-to-date).",
                    Endpoint = "api/v1/Reports/IncomeStatement",
                    Parameters = DateRange,
                },
                new()
                {
                    Key = "balance_sheet", Name = "Balance Sheet", Category = "Accounting",
                    Description = "Assets, liabilities, and equity as of a date.",
                    Endpoint = "api/v1/Reports/BalanceSheet",
                    Parameters = AsOfDateOnly,
                },
                new()
                {
                    Key = "trial_balance", Name = "Trial Balance", Category = "Accounting",
                    Description = "Debit/credit balance for every account as of a date.",
                    Endpoint = "api/v1/Reports/TrialBalance",
                    Parameters = AsOfDateOnly,
                },
            },
        },
        new ReportCategoryDto
        {
            Name = "Sales",
            Reports = new()
            {
                new()
                {
                    Key = "sales_by_customer", Name = "Sales by Customer", Category = "Sales",
                    Description = "Sales totals grouped by customer over a date range.",
                    Endpoint = "api/v1/Reports/SalesByCustomer",
                    Parameters = new()
                    {
                        new() { Name = "date_from", Label = "From date", Type = "date", Required = false },
                        new() { Name = "date_to", Label = "To date", Type = "date", Required = false },
                        new() { Name = "customer_id", Label = "Customer ID", Type = "int", Required = false },
                    },
                },
                new()
                {
                    Key = "sales_by_product", Name = "Sales by Product", Category = "Sales",
                    Description = "Sales totals grouped by product over a date range.",
                    Endpoint = "api/v1/Reports/SalesByProduct",
                    Parameters = new()
                    {
                        new() { Name = "date_from", Label = "From date", Type = "date", Required = false },
                        new() { Name = "date_to", Label = "To date", Type = "date", Required = false },
                        new()
                        {
                            Name = "product_category", Label = "Product category", Type = "select", Required = false,
                            OptionsEndpoint = "api/v1/Reports/ProductCategories",
                        },
                    },
                },
                new()
                {
                    Key = "top_salespeople", Name = "Top Salespeople", Category = "Sales",
                    Description = "Order totals per salesperson over a date range, ranked by sales.",
                    Endpoint = "api/v1/Reports/TopSalespeople",
                    Parameters = DateRange,
                },
            },
        },
        new ReportCategoryDto
        {
            Name = "CRM",
            Reports = new()
            {
                new()
                {
                    Key = "top_opportunities", Name = "Top Opportunities", Category = "CRM",
                    Description = "Open pipeline ranked by weighted value (amount × win %).",
                    Endpoint = "api/v1/Reports/TopOpportunities",
                    Parameters = new()
                    {
                        new() { Name = "top", Label = "Max rows", Type = "int", Required = false },
                    },
                },
            },
        },
        new ReportCategoryDto
        {
            Name = "Shipping",
            Reports = new()
            {
                new()
                {
                    Key = "products_awaiting_shipment", Name = "Products Awaiting Shipment", Category = "Shipping",
                    Description = "Open order lines with quantity still unshipped.",
                    Endpoint = "api/v1/Reports/ProductsAwaitingShipment",
                    Parameters = new(),
                },
                new()
                {
                    Key = "recently_shipped", Name = "Recently Shipped", Category = "Shipping",
                    Description = "Completed shipments over a date range, most recent first.",
                    Endpoint = "api/v1/Reports/RecentlyShipped",
                    Parameters = DateRange,
                },
            },
        },
        new ReportCategoryDto
        {
            Name = "Inventory",
            Reports = new()
            {
                new()
                {
                    Key = "inventory_stock_status", Name = "Inventory Stock Status", Category = "Inventory",
                    Description = "On-hand quantities and valuation across the catalog.",
                    Endpoint = "api/v1/Reports/InventoryStockStatus",
                    Parameters = new()
                    {
                        ProductCategorySelect,
                        new() { Name = "as_of_date", Label = "As of date", Type = "date", Required = false },
                    },
                },
                new()
                {
                    Key = "inventory_reorder", Name = "Inventory Reorder", Category = "Inventory",
                    Description = "Items at or below their reorder point.",
                    Endpoint = "api/v1/Reports/InventoryReorder",
                    Parameters = new()
                    {
                        ProductCategorySelect,
                    },
                },
            },
        },
        new ReportCategoryDto
        {
            Name = "Purchasing",
            Reports = new()
            {
                new()
                {
                    Key = "open_po_receiving", Name = "Open PO / Receiving", Category = "Purchasing",
                    Description = "Outstanding purchase-order quantities awaiting receipt.",
                    Endpoint = "api/v1/Reports/OpenPoReceiving",
                    Parameters = new()
                    {
                        new() { Name = "vendor_id", Label = "Vendor ID", Type = "int", Required = false },
                        new() { Name = "date_from", Label = "From date", Type = "date", Required = false },
                        new() { Name = "date_to", Label = "To date", Type = "date", Required = false },
                    },
                },
                new()
                {
                    Key = "recent_purchase_orders", Name = "Recent Purchase Orders", Category = "Purchasing",
                    Description = "Purchase orders created over a date range, most recent first.",
                    Endpoint = "api/v1/Reports/RecentPurchaseOrders",
                    Parameters = DateRange,
                },
                new()
                {
                    Key = "critical_vendors", Name = "Critical Vendors", Category = "Purchasing",
                    Description = "Vendors flagged critical, with open-PO count and total spend.",
                    Endpoint = "api/v1/Reports/CriticalVendors",
                    Parameters = new(),
                },
            },
        },
    };
}
