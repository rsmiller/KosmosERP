import { BaseDto, DataCommand } from "./base-models";

export class ARInvoiceLineDto extends BaseDto {
  ar_invoice_header_id?: number;
  line_number?: number;
  order_line_id?: number;
  product_id?: number;
  order_qty?: number;
  invoice_qty?: number;
  line_total?: number;
  line_tax?: number;
  is_taxable?: boolean;
  line_description?: string;

  identifier1?: string;
  unit_price?: number;
}

export class ARInvoiceLineListDto extends BaseDto {
  ar_invoice_header_id?: number;
  line_number?: number;
  order_line_id?: number;
  product_id?: number;
  order_qty?: number;
  invoice_qty?: number;
  line_total?: number;
  line_tax?: number;
  is_taxable?: boolean;
  line_description?: string;
}

export class ARInvoiceHeaderDto extends BaseDto {
  invoice_number?: number;
  customer_id?: number;
  order_header_id?: number;
  invoice_date?: string;
  tax_percentage?: number;
  payment_terms?: string;
  invoice_due_date?: string;
  is_taxable?: boolean;
  invoice_total?: number;
  is_paid?: boolean;
  paid_on?: string;
  customer_name?: string;
  order_number?: number;
  order_date?: Date;

  ar_invoice_lines?: ARInvoiceLineDto[];

  payment_terms_name?: string;
  pay_method?: string;

  tax_total?: number;
}

export class ARInvoiceHeaderListDto extends BaseDto {
    invoice_number?: number;
    customer_id?: number;
    order_header_id?: number;
    invoice_date?: Date;
    tax_percentage?: number;
    payment_terms?: string;
    invoice_due_date?: Date;
    is_taxable?: boolean;
    invoice_total?: number;
    is_paid?: boolean;
    paid_on?: Date;
    customer_name?: string;
    order_number?: number;

    payment_terms_name?: string;
}

export class ARInvoiceHeaderCreateCommand extends DataCommand {
  invoice_number?: number;
  customer_id?: number;
  order_header_id?: number;
  invoice_date?: string;
  tax_percentage?: number;
  payment_terms?: string;
  invoice_due_date?: string;
  is_taxable?: boolean;
  invoice_total?: number;
  is_paid?: boolean;
  paid_on?: string;
  ar_invoice_lines?: ARInvoiceLineCreateCommand[];
}

export class ARInvoiceHeaderEditCommand extends DataCommand {
  id?: number;
  invoice_number?: number;
  customer_id?: number;
  order_header_id?: number;
  invoice_date?: string;
  tax_percentage?: number;
  payment_terms?: string;
  invoice_due_date?: string;
  is_taxable?: boolean;
  invoice_total?: number;
  is_paid?: boolean;
  paid_on?: string;
  ar_invoice_lines?: ARInvoiceLineEditCommand[];
}

export class ARInvoiceLineCreateCommand extends DataCommand
{
  line_number?: number;
  order_line_id?: number;
  product_id?: number;
  line_description?: string;
  invoice_qty?: number;
  is_taxable?: boolean = false;

  // UI
  units_ordered?: number;
  max_invoice_qty?: number;
  unit_price?: number = 0;
  line_tax?: number = 0;
  total_price?: number = 0;
  tax_rate?: number = 0;
  shipped_qty?: number = 0;
  already_invoiced_qty?: number = 0;
}

export class ARInvoiceLineEditCommand extends DataCommand {
  id?: number;
  line_number?: number;
  order_line_id?: number;
  product_id?: number;
  line_description?: string;
  invoice_qty?: number;
  is_taxable?: boolean;
}

export class ARInvoiceHeaderDeleteCommand extends DataCommand {
  id?: number;
}

export class ARInvoiceHeaderFindCommand extends DataCommand {
  customer_id?: number;
  wildcard?: string;
}

export class ARInvoiceLineDeleteCommand extends DataCommand {
  id?: number;
}

export class vm_OrdersReadyForInvoicing
{
  order_number?: string;
  order_date?: Date;
  pay_method_name?: string;
  pay_method?: string;
  created_by?: string;
  customer_name?: string;
  customer_guid?: string;
  order_guid?: string;

}

export class vw_PartialInvoices
{
  order_number?: string;
  order_date?: Date;
  pay_method_name?: string;
  pay_method?: string;
  created_by?: string;
  customer_name?: string;
  customer_guid?: string;
  order_guid?: string;
  invoiced_qty?: number;
  sold_qty?: number;
  shipped_qty?: number;
  remaining_qty?: number;
}