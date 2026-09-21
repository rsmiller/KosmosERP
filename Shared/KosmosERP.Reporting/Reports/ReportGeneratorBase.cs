using System.Reflection;
using FastReport;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports;

/// <summary>
/// Shared plumbing for report generators: loading a template from embedded resources and
/// stamping the company header (from the <see cref="Settings"/> entity) onto every report.
/// Company details are always passed as report parameters — never hard-coded in a template.
/// </summary>
public abstract class ReportGeneratorBase : IReportGenerator
{
    protected readonly IBaseERPContext Context;

    protected ReportGeneratorBase(IBaseERPContext context)
    {
        Context = context;
    }

    public abstract string ReportKey { get; }
    public abstract string PermissionToken { get; }
    public abstract string Title { get; }
    public abstract Task<GeneratedReport> GenerateAsync(ReportRequest request);

    /// <summary>
    /// Loads a <c>.frx</c> template embedded in this assembly by its file name
    /// (e.g. "ar_invoice.frx") and returns a <see cref="Report"/> ready for data registration.
    /// </summary>
    protected static Report LoadTemplate(string templateFileName)
    {
        var assembly = typeof(ReportGeneratorBase).Assembly;
        using var stream = assembly.GetManifestResourceStream(templateFileName)
            ?? throw new FileNotFoundException(
                $"Embedded report template '{templateFileName}' was not found. " +
                $"Available: {string.Join(", ", assembly.GetManifestResourceNames())}");

        var report = new Report();
        report.Load(stream);
        return report;
    }

    /// <summary>
    /// Reads the single <see cref="Settings"/> row and sets the standard company-header
    /// parameters shared by every report template.
    /// </summary>
    protected async Task ApplyCompanyHeaderAsync(Report report)
    {
        var settings = await Context.Settings
            .AsNoTracking()
            .Where(s => s.is_deleted == false)
            .OrderBy(s => s.id)
            .FirstOrDefaultAsync();

        report.SetParameterValue("CompanyName", settings?.company_name ?? string.Empty);
        report.SetParameterValue("CompanyAddress", ComposeStreet(settings));
        report.SetParameterValue("CompanyCityStateZip", ComposeCityStateZip(settings?.company_city, settings?.company_state, settings?.company_zip));
        report.SetParameterValue("CompanyPhone", settings?.company_phone ?? string.Empty);
        report.SetParameterValue("CompanyEmail", settings?.company_ar_email ?? settings?.company_general_email ?? string.Empty);
    }

    private static string ComposeStreet(Settings? settings)
    {
        if (settings == null)
            return string.Empty;

        var parts = new[] { settings.company_address1, settings.company_address2 }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    protected static string ComposeCityStateZip(string? city, string? state, string? zip)
    {
        var cityState = string.Join(", ", new[] { city, state }.Where(p => !string.IsNullOrWhiteSpace(p)));
        return string.Join(" ", new[] { cityState, zip }.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();
    }

    /// <summary>
    /// Resolves an <see cref="Address"/> by id into a street line and a "City, ST ZIP" line.
    /// Returns empty strings when the id is missing or the address is not found/deleted.
    /// </summary>
    protected async Task<(string street, string cityStateZip)> ResolveAddressAsync(int addressId)
    {
        if (addressId <= 0)
            return (string.Empty, string.Empty);

        var address = await Context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.id == addressId && a.is_deleted == false);

        if (address == null)
            return (string.Empty, string.Empty);

        var street = string.Join(", ", new[] { address.street_address1, address.street_address2 }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
        return (street, ComposeCityStateZip(address.city, address.state, address.postal_code));
    }

    /// <summary>Reads a parameter as an int, accepting int, string, or numeric inputs.</summary>
    protected static bool TryGetInt(ReportRequest request, string key, out int value)
    {
        value = 0;
        if (!request.Parameters.TryGetValue(key, out var raw) || raw == null)
            return false;

        switch (raw)
        {
            case int i:
                value = i;
                return true;
            case long l:
                value = (int)l;
                return true;
            default:
                return int.TryParse(raw.ToString(), out value);
        }
    }

    /// <summary>Reads a parameter as a <see cref="DateOnly"/> (accepts DateOnly, DateTime, or ISO/parseable string).</summary>
    protected static DateOnly? GetDate(ReportRequest request, string key)
    {
        if (!request.Parameters.TryGetValue(key, out var raw) || raw == null)
            return null;

        switch (raw)
        {
            case DateOnly d:
                return d;
            case DateTime dt:
                return DateOnly.FromDateTime(dt);
            default:
                if (DateOnly.TryParse(raw.ToString(), out var parsed))
                    return parsed;
                if (DateTime.TryParse(raw.ToString(), out var parsedDt))
                    return DateOnly.FromDateTime(parsedDt);
                return null;
        }
    }

    /// <summary>Reads a date parameter, defaulting to today (UTC) when absent or unparseable.</summary>
    protected static DateOnly GetDateOrToday(ReportRequest request, string key)
        => GetDate(request, key) ?? DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Reads a parameter as a string, or null when absent/blank.</summary>
    protected static string? GetString(ReportRequest request, string key)
    {
        if (request.Parameters.TryGetValue(key, out var raw) && raw != null)
        {
            var s = raw.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }
        return null;
    }
}
