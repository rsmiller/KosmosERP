import { BaseDto, DataCommand } from "./base-models";

export class CreditMemoLineDto extends BaseDto {
  credit_memo_header_id?: number;
  line_number?: number;
  line_total?: number;
  qty_credited?: number;
  gl_account_id?: string;
  description?: string;
  product_id?: number;
  ar_invoice_line_id?: number;
  order_line_id?: number;

  product_name?: string;

  // For row tracking
  internal_id: number = Math.floor(Math.random() * 1000000);
}

export class CreditMemoHeaderCreateCommand extends DataCommand {
  customer_id?: number;
  credit_memo_date?: string;
  credit_memo_due_date?: string;
  credit_memo_total?: number;
  memo?: string;
  ar_invoice_header_id?: number;
  order_header_id?: number;
  credit_reason?: string;
  is_approved?: boolean;
  is_applied?: boolean;

  credit_memo_lines?: CreditMemoLineCreateCommand[];

  // For input, not actually used
  ar_ionvoice_number?: string;
}

export class CreditMemoLineCreateCommand extends DataCommand {
  credit_memo_header_id?: number;
  line_number?: number;
  line_total?: number;
  qty_credited?: number;
  gl_account_id?: string;
  description?: string;
  product_id?: number;
  ar_invoice_line_id?: number;
  order_line_id?: number;

  // Fake id
  id: number = 0;
}

export class CreditMemoHeaderDto implements BaseDto {
  id: number = 0;
  customer_id?: number;
  credit_memo_number?: string;
  credit_memo_date?: string;
  credit_memo_due_date?: string;
  credit_memo_total?: number;
  memo?: string;
  credit_reason?: string;
  is_approved?: boolean;
  is_applied?: boolean;
  credit_memo_lines?: CreditMemoLineDto[];
  ar_invoice_header_id?: number;
  order_header_id?: number;

  customer_name?: string;
  order_number?: string;
  ar_invoice_number?: string;
}

export interface CreditMemoHeaderListDto extends BaseDto {
  credit_memo_number?: string;
  customer_id?: number;
  credit_memo_date?: string;
  credit_memo_total?: number;
  customer_name?: string;
  guid?: string;
  ar_invoice_header_id?: number;
  order_header_id?: number;
}

export class CreditMemoHeaderFindCommand extends DataCommand {
  customer_id?: number;
  wildcard?: string;
}

export class CreditMemoLineEditCommand extends DataCommand {
  id?: number;
  line_number?: number;
  line_total?: number;
  qty_credited?: number;
  gl_account_id?: string;
  description?: string;
  product_id?: number;
  ar_invoice_line_id?: number;
  order_line_id?: number;

  // For row tracking
  internal_id: number = Math.floor(Math.random() * 1000000);
}

export class CreditMemoHeaderEditCommand extends DataCommand {
  id: number = 0;
  customer_id?: number;
  credit_memo_date?: string;
  credit_memo_due_date?: string;
  credit_memo_total?: number;
  memo?: string;
  ar_invoice_header_id?: number;
  order_header_id?: number;
  credit_reason?: string;
  is_approved?: boolean;
  is_applied?: boolean;

  credit_memo_lines?: CreditMemoLineEditCommand[];

  customer_name?: string;
  ar_invoice_number?: string;
}

export class CreditMemoHeaderDeleteCommand extends DataCommand {
  id?: number;
}

export class CreditMemoLineDeleteCommand extends DataCommand {
  id?: number;
}