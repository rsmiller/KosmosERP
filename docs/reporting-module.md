# Reporting Module (FastReport)

## Overview
The Reporting Module renders KosmosERP business documents and analytical reports to **PDF** or
**HTML** using [FastReport.OpenSource](https://www.nuget.org/packages/FastReport.OpenSource)
(MIT-licensed). It exposes 16 reports across Sales, Inventory, Manufacturing, Purchasing and
Accounting through authenticated API endpoints.

## Licensing
Only the **MIT** `FastReport.OpenSource*` packages are used — never the commercial
`FastReport.NET` — so there is no copyleft conflict with the AGPL-3.0 codebase.

| Package | Purpose |
|---|---|
| `FastReport.OpenSource` | Report engine + HTML export |
| `FastReport.OpenSource.Export.PdfSimple` | PDF export (no encryption/signing/font-embedding) |

Native XLSX/DOCX export and the online designer are commercial-only and intentionally **not**
used. For tabular "export to Excel", emit CSV/HTML from the existing module queries instead.

## Architecture
```
Shared/KosmosERP.Reporting/
  IReportService / ReportService        # dispatch by report key; centralized PDF/HTML export
  ReportRequest / ReportResult / ReportFormat
  ReportingModule                       # ERP module identity + report_* permission seeding
  DependencyInjection (AddReporting)    # DI registration (one line per generator)
  Reports/
    IReportGenerator / ReportGeneratorBase   # embedded-template load, company header from Settings
    Accounting/ | Sales/ | Purchasing/ | Inventory/ | Manufacturing/
  Templates/*.frx                       # FastReport XML templates (embedded resources)
Api/Controllers/ReportsController.cs    # explicit per-report endpoints + generic dispatcher
```

- **Data binding**: each generator queries EF Core into flat lists, `RegisterData`s them, sets
  header/company parameters (company info comes from the `Settings` entity — never hard-coded),
  calls `Prepare()`; `ReportService` exports to the requested format.
- **Auth**: `ReportsController` derives from `ERPApiController` and injects `IReportingModule`, so
  `[ERPAuthorize]` resolves the Reporting module id and enforces its **Read** permission exactly
  like every other controller. Report tokens (`report_*`) are documented per endpoint.

## Cross-platform rendering (Linux)
FastReport draws via `System.Drawing.Common`, which P/Invokes **libgdiplus** on Linux. Kosmos
runs in Linux containers, so:

- The runtime Docker image is **Debian slim** (`aspnet:10.0`) with `libgdiplus` + fonts installed
  (Alpine/musl is unreliable for libgdiplus).
- CI installs `libgdiplus` and sets `REQUIRE_REPORT_RENDER=1` so the headless render tests are
  **enforced** there (they skip cleanly on dev machines without the native library).

## API
Base route: `GET /api/v1/Reports/...`. Every report supports `?format=pdf` (default) or
`?format=html`. `GET /api/v1/Reports/Available` lists the report keys. A generic dispatcher
`GET /api/v1/Reports/Generate/{reportKey}` is also available (query-string parameters pass through).

| # | Report | Endpoint | Key / permission | Parameters |
|---|--------|----------|------------------|------------|
| 1 | Sales Order Acknowledgement | `SalesOrderAcknowledgement` | `report_sales_order_ack` | `order_id` or `order_guid` |
| 2 | Packing Slip | `PackingSlip` | `report_packing_slip` | `shipment_id` |
| 3 | Sales by Customer | `SalesByCustomer` | `report_sales_by_customer` | `date_from`, `date_to`, `customer_id?` |
| 4 | Sales by Product | `SalesByProduct` | `report_sales_by_product` | `date_from`, `date_to`, `product_category?` |
| 5 | Inventory Stock Status | `InventoryStockStatus` | `report_inventory_stock_status` | `as_of_date?`, `category?` |
| 6 | Inventory Reorder | `InventoryReorder` | `report_inventory_reorder` | `category?` |
| 7 | Bill of Materials | `Bom` | `report_bom` | `product_id` or `bom_id` |
| 8 | Production Order Traveler | `ProductionOrderTraveler` | `report_production_order_traveler` | `production_order_id` |
| 9 | Purchase Order | `PurchaseOrder` | `report_purchase_order` | `purchase_order_id` or `purchase_order_guid` |
| 10 | Open PO / Receiving | `OpenPoReceiving` | `report_open_po_receiving` | `vendor_id?`, `date_from?`, `date_to?` |
| 11 | AR Invoice | `ArInvoice` | `report_ar_invoice` | `ar_invoice_id` or `ar_invoice_guid` |
| 12 | AR Aging | `ArAging` | `report_ar_aging` | `as_of_date?` |
| 13 | AP Aging | `ApAging` | `report_ap_aging` | `as_of_date?` |
| 14 | Income Statement (P&L) | `IncomeStatement` | `report_income_statement` | `date_from`, `date_to` |
| 15 | Balance Sheet | `BalanceSheet` | `report_balance_sheet` | `as_of_date?` |
| 16 | Trial Balance | `TrialBalance` | `report_trial_balance` | `as_of_date?` |

Financial statements (#14–16) are driven off the `FinancialTransaction` posting ledger joined to
`ChartOfAccount` (types: Asset/Liability/Equity/Revenue/Expense). Aging (#12/#13) buckets open
balances into Current / 1–30 / 31–60 / 61–90 / 90+ by days past due.

Example:
```
GET /api/v1/Reports/ArInvoice?ar_invoice_id=42&format=pdf
GET /api/v1/Reports/TrialBalance?as_of_date=2026-12-31&format=html
```

## Adding a report
1. Add a generator in `Reports/<Area>/` deriving `ReportGeneratorBase` (implement `ReportKey`,
   `PermissionToken`, `Title`, `GenerateAsync`).
2. Author `Templates/<key>.frx` (embedded automatically by the csproj glob).
3. Register it in `DependencyInjection.AddReporting()` (one line).
4. Add an explicit endpoint in `ReportsController` and a render test.

## Testing
Tests live in `Tests/KosmosERP.Tests.Modules` (EF Core InMemory). Each report has render tests
asserting `Success`, `ContentType`, and a valid `%PDF` header; aging and financial reports also
assert numeric correctness against hand-computed fixtures (buckets, trial-balance ties,
net income, and Assets = Liabilities + Equity). Render tests use `ReportRenderGuard` to skip when
libgdiplus is absent locally while staying enforced in CI.

## Known limitations / TODO
- **Per-group subtotals** (inventory by category, open-PO by vendor) are shown with a grouping
  column and a grand total; FastReport group bands could add per-group subtotals.
- **Multi-level BOM explosion** (#7) and the traveler's components (#8) are single-level.
- **Production traveler barcode** (#8): the traveler number prints as text; a barcode object can be added.
- **P&L COGS breakout** (#14): the chart of accounts has no COGS sub-classification, so expenses
  are shown as one group (Revenue / Expense / Net Income).
- **Inventory location** grouping (#5) is omitted — there is no location/warehouse field on
  `Product`/`Inventory`.
- **General Ledger Detail** variant of the trial balance (transaction-level rows off
  `JournalEntryLine`) is not yet implemented.
