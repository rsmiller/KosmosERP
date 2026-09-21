using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Purchasing;

/// <summary>
/// Critical Vendors — vendors flagged critical (<c>is_critial_vendor</c>), each augmented with the
/// count of their currently open purchase orders and their total purchasing spend, ranked by spend.
/// Takes no parameters. (The flag's field name preserves the repo's original spelling.)
/// </summary>
public sealed class CriticalVendorsReport : ReportGeneratorBase
{
    public const string Key = "critical_vendors";
    private const string MoneyFormat = "#,##0.00";

    public CriticalVendorsReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_critical_vendors";
    public override string Title => "Critical Vendors";

    public sealed class DisplayRow
    {
        public int VendorNumber { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OpenPOs { get; set; }
        public string TotalSpend { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var vendors = await Context.Vendors
            .AsNoTracking()
            .Where(v => v.is_deleted == false && v.is_critial_vendor == true)
            .Select(v => new { v.id, v.vendor_number, v.vendor_name, v.category, v.phone, v.general_email })
            .ToListAsync();

        var vendorIds = vendors.Select(v => v.id).ToList();

        var pos = await Context.PurchaseOrderHeaders
            .AsNoTracking()
            .Where(p => p.is_deleted == false && p.is_canceled == false && vendorIds.Contains(p.vendor_id))
            .Select(p => new { p.vendor_id, p.price, p.is_complete })
            .ToListAsync();

        var byVendor = pos
            .GroupBy(p => p.vendor_id)
            .ToDictionary(
                g => g.Key,
                g => new { OpenPOs = g.Count(x => !x.is_complete), Spend = g.Sum(x => x.price) });

        var rows = vendors
            .Select(v =>
            {
                byVendor.TryGetValue(v.id, out var agg);
                return new
                {
                    Vendor = v,
                    OpenPOs = agg?.OpenPOs ?? 0,
                    Spend = agg?.Spend ?? 0m,
                };
            })
            .OrderByDescending(x => x.Spend)
            .ThenBy(x => x.Vendor.vendor_name)
            .Select(x => new DisplayRow
            {
                VendorNumber = x.Vendor.vendor_number,
                Vendor = x.Vendor.vendor_name,
                Category = x.Vendor.category,
                Phone = x.Vendor.phone,
                Email = x.Vendor.general_email ?? string.Empty,
                OpenPOs = x.OpenPOs,
                TotalSpend = x.Spend.ToString(MoneyFormat),
            })
            .ToList();

        var grandVendors = vendors.Count;
        var grandSpend = pos.Sum(p => p.price);

        var report = LoadTemplate("critical_vendors.frx");
        report.RegisterData(rows, "Vendors");
        var ds = report.GetDataSource("Vendors");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("GeneratedText", $"As of {DateTime.UtcNow:yyyy-MM-dd}");
        report.SetParameterValue("GrandVendors", grandVendors.ToString());
        report.SetParameterValue("GrandSpend", grandSpend.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Critical_Vendors_{DateTime.UtcNow:yyyyMMdd}"
        };
    }
}
