using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.Seeder;

/// <summary>
/// Milestone 4 — accounting. Seeds a standard chart of accounts, AR invoices (+ payments) and
/// AP invoices, and — because direct inserts bypass the modules' auto-posting — writes the
/// matching <c>financial_transactions</c> (debit/credit) so the GL-based statements
/// (Trial Balance, Income Statement, Balance Sheet) render, alongside AR/AP aging.
/// </summary>
public partial class DatabaseSeeder
{
    private static class Acct
    {
        public const string Cash = "1000";
        public const string AccountsReceivable = "1100";
        public const string Inventory = "1200";
        public const string AccountsPayable = "2010";
        public const string OwnersEquity = "3000";
        public const string SalesRevenue = "4010";
        public const string Cogs = "5000";
        public const string Purchases = "5010";
        public const string OperatingExpenses = "6000";
    }

    private async Task SeedChartOfAccountsAsync()
    {
        // (number, name, account_type, normal_balance)
        var accounts = new (string Number, string Name, int Type, int Normal)[]
        {
            (Acct.Cash, "Cash", AccountType.Asset, NormalBalance.Debit),
            (Acct.AccountsReceivable, "Accounts Receivable", AccountType.Asset, NormalBalance.Debit),
            (Acct.Inventory, "Inventory", AccountType.Asset, NormalBalance.Debit),
            (Acct.AccountsPayable, "Accounts Payable", AccountType.Liability, NormalBalance.Credit),
            (Acct.OwnersEquity, "Owner's Equity", AccountType.Equity, NormalBalance.Credit),
            (Acct.SalesRevenue, "Sales Revenue", AccountType.Revenue, NormalBalance.Credit),
            (Acct.Cogs, "Cost of Goods Sold", AccountType.Expense, NormalBalance.Debit),
            (Acct.Purchases, "Purchases", AccountType.Expense, NormalBalance.Debit),
            (Acct.OperatingExpenses, "Operating Expenses", AccountType.Expense, NormalBalance.Debit),
        };

        foreach (var a in accounts)
        {
            var account = Stamp(new ChartOfAccount
            {
                account_number = a.Number,
                account_name = a.Name,
                account_type = a.Type,
                normal_balance = a.Normal,
                is_active = true,
            });
            _context.ChartOfAccounts.Add(account);
        }
        await _context.SaveChangesAsync();

        foreach (var account in _context.ChartOfAccounts)
            _accountIdByNumber[account.account_number] = account.id;

        Console.WriteLine($"  Chart of accounts seeded ({accounts.Length}).");
    }

    private async Task SeedInvoicesAndPaymentsAsync()
    {
        // Opening balance: Dr Cash / Cr Owner's Equity.
        var openingDate = DateTime.UtcNow.AddYears(-1);
        AddFinancial(openingDate, FinancialTransactionType.JournalEntry, "Manual", 0, Guid.NewGuid().ToString(), Acct.Cash, debit: 250000m);
        AddFinancial(openingDate, FinancialTransactionType.JournalEntry, "Manual", 0, Guid.NewGuid().ToString(), Acct.OwnersEquity, credit: 250000m);

        // ---- AR invoices + payments (for non-canceled orders) ----
        var invoiceNumber = DatabaseStartNumbers.ARInvoices;
        var paymentNumber = DatabaseStartNumbers.Payments;
        var arCount = 0;
        var payCount = 0;

        var invoiceable = _orders.Where(o => !o.Canceled).ToList();
        for (var i = 0; i < invoiceable.Count; i++)
        {
            var order = invoiceable[i];
            var total = order.Lines.Sum(l => l.Qty * l.UnitPrice);
            var cost = order.Lines.Sum(l => l.Qty * (_productById.TryGetValue(l.ProductId, out var p) ? p.our_cost : 0m));
            if (total <= 0) continue;

            var invoiceDate = order.OrderDate.AddDays(3);
            var dueDate = invoiceDate.AddDays(30);

            // payment status mix: 0,1 = paid full; 2 = partial; else unpaid.
            var payMode = i % 5;
            var isPaidFull = payMode is 0 or 1;

            var header = Stamp(new ARInvoiceHeader
            {
                customer_id = order.CustomerId,
                order_header_id = order.Id,
                invoice_number = invoiceNumber++,
                payment_terms = Kv.TermsNet30,
                invoice_date = invoiceDate,
                invoice_due_date = dueDate,
                invoice_total = total,
                tax_percentage = 0m,
                is_paid = isPaidFull,
                is_taxable = false,
                is_posted = true,
                paid_on = isPaidFull ? dueDate.AddDays(-2) : null,
            });
            _context.ARInvoiceHeaders.Add(header);
            await _context.SaveChangesAsync();

            var lineNo = 1;
            foreach (var line in order.Lines)
            {
                var product = _productById[line.ProductId];
                _context.ARInvoiceLines.Add(Stamp(new ARInvoiceLine
                {
                    ar_invoice_header_id = header.id,
                    line_number = lineNo++,
                    order_line_id = line.Id,
                    product_id = line.ProductId,
                    line_description = product.product_name,
                    order_qty = line.Qty,
                    invoice_qty = line.Qty,
                    line_total = line.Qty * line.UnitPrice,
                }));
            }
            await _context.SaveChangesAsync();
            arCount++;

            // GL postings: Dr AR / Cr Sales, and Dr COGS / Cr Inventory.
            var txnDate = invoiceDate.ToDateTime(TimeOnly.MinValue);
            AddFinancial(txnDate, FinancialTransactionType.ARPost, "ARInvoice", header.id, header.guid, Acct.AccountsReceivable, debit: total);
            AddFinancial(txnDate, FinancialTransactionType.ARPost, "ARInvoice", header.id, header.guid, Acct.SalesRevenue, credit: total);
            if (cost > 0)
            {
                AddFinancial(txnDate, FinancialTransactionType.ARPost, "ARInvoice", header.id, header.guid, Acct.Cogs, debit: cost);
                AddFinancial(txnDate, FinancialTransactionType.ARPost, "ARInvoice", header.id, header.guid, Acct.Inventory, credit: cost);
            }

            // Payments.
            var payAmount = isPaidFull ? total : payMode == 2 ? Math.Round(total * 0.5m, 2) : 0m;
            if (payAmount > 0)
            {
                _context.Payments.Add(Stamp(new Payment
                {
                    order_header_id = order.Id,
                    ar_header_id = header.id,
                    payment_number = paymentNumber++,
                    payment_amount = payAmount,
                    transaction_method = "Check",
                    transaction_date = txnDate.AddDays(10),
                    transaction_status = "completed",
                    guid = Guid.NewGuid().ToString(),
                }));
                payCount++;
            }
        }
        await _context.SaveChangesAsync();

        // ---- AP invoices (for received POs) ----
        var apCount = 0;
        var apInvoiceSeq = 7000;
        var receivedPos = _purchaseOrders.Where(po => po.Lines.Any(l => l.ReceivedQty > 0)).ToList();
        for (var i = 0; i < receivedPos.Count; i++)
        {
            var po = receivedPos[i];
            var total = po.Lines.Sum(l => l.ReceivedQty * l.UnitPrice);
            if (total <= 0) continue;

            var isPaid = i % 3 == 0;
            var invoiceDate = po.CreatedOn.AddDays(7);

            var header = Stamp(new APInvoiceHeader
            {
                vendor_id = po.VendorId,
                invoice_number = $"AP-{apInvoiceSeq++}",
                invoice_date = invoiceDate,
                invoice_due_date = invoiceDate.AddDays(30),
                invoice_received_date = invoiceDate,
                invoice_total = total,
                purchase_order_receive_id = null,
                association_is_purchase_order = true,
                association_object_id = po.Id,
                is_paid = isPaid,
                is_posted = true,
            });
            _context.APInvoiceHeaders.Add(header);
            await _context.SaveChangesAsync();

            var lineNo = 1;
            foreach (var line in po.Lines.Where(l => l.ReceivedQty > 0))
            {
                _context.APInvoiceLines.Add(Stamp(new APInvoiceLine
                {
                    ap_invoice_header_id = header.id,
                    line_number = lineNo++,
                    line_total = line.ReceivedQty * line.UnitPrice,
                    qty_invoiced = line.ReceivedQty,
                    gl_account = Acct.Purchases,
                    description = _productById.TryGetValue(line.ProductId, out var p) ? p.product_name : "Component",
                    association_is_purchase_order = true,
                    association_object_id = po.Id,
                }));
            }
            await _context.SaveChangesAsync();
            apCount++;

            // GL postings: Dr Purchases / Cr AP.
            AddFinancial(invoiceDate, FinancialTransactionType.APPost, "APInvoice", header.id, header.guid, Acct.Purchases, debit: total);
            AddFinancial(invoiceDate, FinancialTransactionType.APPost, "APInvoice", header.id, header.guid, Acct.AccountsPayable, credit: total);
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  Accounting seeded ({arCount} AR invoices, {payCount} payments, {apCount} AP invoices, {_context.FinancialTransactions.Count()} GL rows).");
    }

    private void AddFinancial(DateTime date, int type, string module, int sourceId, string sourceGuid, string accountNumber, decimal debit = 0, decimal credit = 0)
    {
        if (!_accountIdByNumber.TryGetValue(accountNumber, out var accountId))
            return;

        _context.FinancialTransactions.Add(Stamp(new FinancialTransaction
        {
            transaction_date = date,
            transaction_type = type,
            source_module = module,
            source_id = sourceId,
            source_guid = sourceGuid,
            chart_of_account_id = accountId,
            debit_amount = debit,
            credit_amount = credit,
            running_balance = 0m,
            fiscal_period = date.ToString("yyyy-MM"),
            is_reversal = false,
        }));
    }
}
