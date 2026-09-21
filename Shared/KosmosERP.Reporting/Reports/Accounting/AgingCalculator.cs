namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// One aging line (per customer or per vendor): an open balance split across the standard
/// buckets Current / 1–30 / 31–60 / 61–90 / 90+, based on days past the due date.
/// </summary>
public sealed class AgingRow
{
    public int EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Days90Plus { get; set; }
    public decimal Total { get; set; }
}

/// <summary>Shared aging-bucket logic used by AR (#12) and AP (#13) aging.</summary>
public static class AgingCalculator
{
    /// <summary>
    /// Adds an open <paramref name="amount"/> to the right bucket on <paramref name="row"/>,
    /// given how many days past due it is as of the report date. Not-yet-due (≤ 0) is Current.
    /// </summary>
    public static void Add(AgingRow row, decimal amount, int daysPastDue)
    {
        if (daysPastDue <= 0)
            row.Current += amount;
        else if (daysPastDue <= 30)
            row.Days1To30 += amount;
        else if (daysPastDue <= 60)
            row.Days31To60 += amount;
        else if (daysPastDue <= 90)
            row.Days61To90 += amount;
        else
            row.Days90Plus += amount;

        row.Total += amount;
    }

    /// <summary>Sums a set of aging rows into a single grand-total row.</summary>
    public static AgingRow GrandTotal(IEnumerable<AgingRow> rows, string label = "Grand Total")
    {
        var total = new AgingRow { Name = label };
        foreach (var r in rows)
        {
            total.Current += r.Current;
            total.Days1To30 += r.Days1To30;
            total.Days31To60 += r.Days31To60;
            total.Days61To90 += r.Days61To90;
            total.Days90Plus += r.Days90Plus;
            total.Total += r.Total;
        }
        return total;
    }
}
