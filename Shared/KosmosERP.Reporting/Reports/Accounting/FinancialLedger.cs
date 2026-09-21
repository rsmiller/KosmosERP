using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// An account's posted activity: total debits and credits (from <c>FinancialTransaction</c>),
/// with its chart-of-account metadata. <see cref="Net"/> is debit − credit.
/// </summary>
public sealed class AccountBalance
{
    public int AccountId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AccountType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    /// <summary>Debit − credit. Positive for a net debit balance, negative for a net credit balance.</summary>
    public decimal Net => Debit - Credit;
}

/// <summary>
/// Computes account balances from the <c>FinancialTransaction</c> posting ledger joined to
/// <c>ChartOfAccount</c>. Used by Trial Balance (#16), Income Statement (#14) and Balance Sheet (#15).
/// </summary>
public static class FinancialLedger
{
    /// <summary>Balances for transactions on or before <paramref name="asOf"/> (Balance Sheet / Trial Balance).</summary>
    public static Task<List<AccountBalance>> BalancesAsOfAsync(IBaseERPContext context, DateOnly asOf)
    {
        var asOfEnd = asOf.ToDateTime(TimeOnly.MaxValue);
        return AggregateAsync(context, ft => ft.transaction_date <= asOfEnd);
    }

    /// <summary>Balances for transactions within [<paramref name="from"/>, <paramref name="to"/>] (Income Statement).</summary>
    public static Task<List<AccountBalance>> BalancesInRangeAsync(IBaseERPContext context, DateOnly from, DateOnly to)
    {
        var start = from.ToDateTime(TimeOnly.MinValue);
        var end = to.ToDateTime(TimeOnly.MaxValue);
        return AggregateAsync(context, ft => ft.transaction_date >= start && ft.transaction_date <= end);
    }

    private static async Task<List<AccountBalance>> AggregateAsync(
        IBaseERPContext context,
        System.Linq.Expressions.Expression<Func<Database.Models.FinancialTransaction, bool>> dateFilter)
    {
        var grouped = await context.FinancialTransactions
            .AsNoTracking()
            .Where(ft => ft.is_deleted == false)
            .Where(dateFilter)
            .GroupBy(ft => ft.chart_of_account_id)
            .Select(g => new
            {
                AccountId = g.Key,
                Debit = g.Sum(x => x.debit_amount),
                Credit = g.Sum(x => x.credit_amount)
            })
            .ToListAsync();

        var accountIds = grouped.Select(g => g.AccountId).ToList();
        var accounts = await context.ChartOfAccounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.id))
            .ToDictionaryAsync(a => a.id);

        var result = new List<AccountBalance>();
        foreach (var g in grouped)
        {
            accounts.TryGetValue(g.AccountId, out var acct);
            result.Add(new AccountBalance
            {
                AccountId = g.AccountId,
                Number = acct?.account_number ?? string.Empty,
                Name = acct?.account_name ?? $"Account {g.AccountId}",
                AccountType = acct?.account_type ?? 0,
                Debit = g.Debit,
                Credit = g.Credit
            });
        }

        return result.OrderBy(r => r.Number).ToList();
    }
}
