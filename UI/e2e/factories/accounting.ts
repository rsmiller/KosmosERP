import type { APInvoiceHeaderDto, APInvoiceLineDto } from '@/models/ap-models';
import type {
  ARInvoiceHeaderDto,
  ARInvoiceLineDto,
  vm_OrdersReadyForInvoicing,
  vw_PartialInvoices,
} from '@/models/ar-models';
import type { ChartOfAccountDto } from '@/models/chart-of-account-models';
import type { CreditMemoHeaderDto, CreditMemoLineDto } from '@/models/credit-memo-models';
import type { FinancialTransactionDto } from '@/models/financial-transaction-models';
import type { JournalEntryHeaderDto, JournalEntryLineDto } from '@/models/journal-entry-models';
import { GLAccountKeys, PaymentTermKeys } from './lookups';
import { baseDto, DATE_ONLY, DUE_DATE_ONLY, FIXED_DATE, nextId } from './sequence';

export function buildARInvoiceLine(overrides: Partial<ARInvoiceLineDto> = {}): ARInvoiceLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('arln', id),
    ar_invoice_header_id: 1,
    line_number: 1,
    order_line_id: 1,
    product_id: 1,
    order_qty: 4,
    invoice_qty: 4,
    unit_price: 1800,
    line_total: 7200,
    line_tax: 0,
    is_taxable: false,
    line_description: 'Workstation build',
    identifier1: 'SKU-1',
    ...overrides,
  } as ARInvoiceLineDto;
}

export function buildARInvoice(overrides: Partial<ARInvoiceHeaderDto> = {}): ARInvoiceHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('arin', id),
    invoice_number: 60000 + id,
    customer_id: 1,
    customer_name: 'Customer',
    order_header_id: 1,
    order_number: 50001,
    invoice_date: DATE_ONLY,
    invoice_due_date: DUE_DATE_ONLY,
    tax_percentage: 0,
    tax_total: 0,
    payment_terms: PaymentTermKeys.Net30,
    payment_terms_name: 'Net 30',
    is_taxable: false,
    invoice_total: 7200,
    is_paid: false,
    ar_invoice_lines: [],
    ...overrides,
  } as ARInvoiceHeaderDto;
}

export function buildOrderReadyForInvoicing(overrides: Partial<vm_OrdersReadyForInvoicing> = {}): vm_OrdersReadyForInvoicing {
  return {
    order_number: '50001',
    pay_method: 'pay_method_net_terms',
    pay_method_name: 'Net Terms',
    created_by: 'Ada Admin',
    customer_name: 'Customer',
    customer_guid: '',
    order_guid: '',
    ...overrides,
  };
}

export function buildPartialInvoice(overrides: Partial<vw_PartialInvoices> = {}): vw_PartialInvoices {
  return {
    ...buildOrderReadyForInvoicing(),
    invoiced_qty: 2,
    sold_qty: 4,
    shipped_qty: 4,
    remaining_qty: 2,
    ...overrides,
  };
}

export function buildAPInvoiceLine(overrides: Partial<APInvoiceLineDto> = {}): APInvoiceLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('apln', id),
    ap_invoice_header_id: 1,
    line_number: 1,
    line_total: 950,
    qty_invoiced: 10,
    units_ordered: 10,
    units_received: 10,
    gl_account: GLAccountKeys.Purchases,
    description: 'DDR5 32GB kit',
    association_is_purchase_order: true,
    association_is_sales_order: false,
    association_is_ar_invoice: false,
    receive_lines: [],
    ...overrides,
  } as APInvoiceLineDto;
}

export function buildAPInvoice(overrides: Partial<APInvoiceHeaderDto> = {}): APInvoiceHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('apin', id),
    vendor_id: 1,
    vendor_name: 'Vendor',
    invoice_number: `VINV-${id}`,
    invoice_date: '2026-03-02T00:00:00',
    invoice_due_date: '2026-04-01T00:00:00',
    invoice_received_date: '2026-03-03T00:00:00',
    invoice_total: 950,
    memo: 'March components',
    association_object_id: 1,
    association_number: 30001,
    association_is_purchase_order: true,
    association_is_sales_order: false,
    association_is_ar_invoice: false,
    packing_list_is_required: false,
    is_paid: false,
    ap_invoice_lines: [],
    po_lines: [],
    order_lines: [],
    ar_lines: [],
    receive_lines: [],
    ...overrides,
  } as unknown as APInvoiceHeaderDto;
}

export function buildCreditMemoLine(overrides: Partial<CreditMemoLineDto> = {}): CreditMemoLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('cmln', id),
    internal_id: id,
    credit_memo_header_id: 1,
    line_number: 1,
    line_total: 1800,
    qty_credited: 1,
    gl_account_id: GLAccountKeys.Revenue,
    description: 'Returned unit',
    product_id: 1,
    product_name: 'Product',
    ar_invoice_line_id: 1,
    order_line_id: 1,
    ...overrides,
  } as CreditMemoLineDto;
}

export function buildCreditMemo(overrides: Partial<CreditMemoHeaderDto> = {}): CreditMemoHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('cmem', id),
    customer_id: 1,
    customer_name: 'Customer',
    credit_memo_number: `CM-${id}`,
    credit_memo_date: FIXED_DATE,
    credit_memo_due_date: '2026-04-01T00:00:00',
    credit_memo_total: 1800,
    memo: 'Returned unit',
    credit_reason: 'Damaged in transit',
    is_approved: false,
    is_applied: false,
    ar_invoice_header_id: 1,
    ar_invoice_number: '60001',
    order_header_id: 1,
    order_number: '50001',
    credit_memo_lines: [],
    ...overrides,
  } as CreditMemoHeaderDto;
}

export function buildChartOfAccount(overrides: Partial<ChartOfAccountDto> = {}): ChartOfAccountDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('coa', id),
    account_number: String(4000 + id),
    account_name: `Account ${id}`,
    account_type: 4,
    account_type_name: 'Revenue',
    is_active: true,
    normal_balance: 2,
    normal_balance_name: 'Credit',
    description: `Ledger account ${id}`,
    ...overrides,
  } as unknown as ChartOfAccountDto;
}

export function buildJournalEntryLine(overrides: Partial<JournalEntryLineDto> = {}): JournalEntryLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('jeln', id),
    journal_entry_header_id: 1,
    line_number: 1,
    chart_of_account_id: 1,
    debit_amount: 100,
    credit_amount: 0,
    description: 'Adjustment',
    account_number: '4001',
    account_name: 'Account',
    ...overrides,
  } as JournalEntryLineDto;
}

export function buildJournalEntry(overrides: Partial<JournalEntryHeaderDto> = {}): JournalEntryHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('jent', id),
    entry_number: 90000 + id,
    entry_date: FIXED_DATE,
    description: `Month-end adjustment ${id}`,
    reference_type: 0,
    reference_type_name: 'Manual',
    is_posted: false,
    is_reversed: false,
    fiscal_period: '2026-03',
    total_debits: 100,
    total_credits: 100,
    lines: [],
    ...overrides,
  } as unknown as JournalEntryHeaderDto;
}

export function buildFinancialTransaction(overrides: Partial<FinancialTransactionDto> = {}): FinancialTransactionDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('fitx', id),
    transaction_date: FIXED_DATE,
    transaction_type: 1,
    transaction_type_name: 'Invoice',
    source_module: 'ARInvoice',
    source_id: 1,
    chart_of_account_id: 1,
    debit_amount: 0,
    credit_amount: 7200,
    running_balance: 7200,
    description: `Posting ${id}`,
    fiscal_period: '2026-03',
    is_reversal: false,
    account_number: '4001',
    account_name: 'Account',
    ...overrides,
  } as unknown as FinancialTransactionDto;
}
