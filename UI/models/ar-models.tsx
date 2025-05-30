import { BaseDto } from "./base-models";

export interface ARInvoiceLineDto extends BaseDto {
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

export interface ARInvoiceLineListDto extends BaseDto {
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

export interface ARInvoiceHeaderDto extends BaseDto {
    invoice_number?: number;
    customer_id?: number;
    order_header_id?: number;
    invoice_date?: Date;
    tax_percentage?: number;
    payment_terms?: number;
    invoice_due_date?: Date;
    is_taxable?: boolean;
    invoice_total?: number;
    is_paid?: boolean;
    paid_on?: Date;
    customer_name?: string;
    order_number?: number;

    ar_invoice_lines?: ARInvoiceLineDto[];
}

export interface ARInvoiceHeaderListDto extends BaseDto {
    invoice_number?: number;
    customer_id?: number;
    order_header_id?: number;
    invoice_date?: Date;
    tax_percentage?: number;
    payment_terms?: number;
    invoice_due_date?: Date;
    is_taxable?: boolean;
    invoice_total?: number;
    is_paid?: boolean;
    paid_on?: Date;
    customer_name?: string;
    order_number?: number;
}