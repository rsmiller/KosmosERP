import { BaseDto } from "./base-models";

export interface PurchaseOrderLineDto extends BaseDto {
  purchase_order_header_id?: number;
  product_id?: number;
  revision_number?: number;
  line_number?: number;
  quantity?: number;
  description?: string;
  unit_price?: number;
  tax?: number;
  is_taxable?: boolean;
  is_complete?: boolean;
  is_canceled?: boolean;

  product_name?: string;
}

export interface PurchaseOrderHeaderDto extends BaseDto {
  vendor_id?: number;
  po_type?: string;
  revision_number?: number;
  po_number?: number;
  price?: number;
  tax?: number;
  po_quote_number?: string | null;
  deleted_reason?: string | null;
  canceled_reason?: string | null;
  is_complete?: boolean;
  is_canceled?: boolean;
  completed_on?: Date | null;
  completed_by?: number | null;
  canceled_on?: Date | null;
  canceled_by?: number | null;
  purchase_order_lines: PurchaseOrderLineDto[];

  vendor_name?: string;
}

export interface PurchaseOrderHeaderListDto extends BaseDto {
  vendor_id?: number;
  po_type?: string;
  revision_number?: number;
  po_number?: number;
  price?: number;
  tax?: number;
  po_quote_number?: string | null;
  deleted_reason?: string | null;
  canceled_reason?: string | null;
  is_complete?: boolean;
  is_canceled?: boolean;
  completed_on?: Date | null;
  completed_by?: number | null;
  canceled_on?: Date | null;
  canceled_by?: number | null;

  vendor_name?: string;
}

