import { BaseDto, DataCommand } from "./base-models";

export class PurchaseOrderLineDto extends BaseDto {
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
  identifier1?: string;
}

export class PurchaseOrderHeaderDto extends BaseDto {
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
  purchase_order_lines?: PurchaseOrderLineDto[];

  vendor_name?: string;
  po_by?: string;
}

export class PurchaseOrderHeaderListDto extends BaseDto {
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
  po_by?: string;
}

export class PurchaseOrderHeaderCreateCommand extends DataCommand {
  vendor_id?: number;
  po_type?: string;
  revision_number?: number;
  po_number?: number;
  price?: number;
  tax?: number;
  po_quote_number?: string;
  deleted_reason?: string;
  canceled_reason?: string;
  is_complete?: boolean;
  is_canceled?: boolean;
  completed_on?: Date;
  completed_by?: number;
  canceled_on?: Date;
  canceled_by?: number;
  purchase_order_lines?: PurchaseOrderLineCreateCommand[];
}

export class PurchaseOrderHeaderEditCommand extends DataCommand {
  id?: number;
  vendor_id?: number;
  po_type?: string;
  revision_number?: number;
  po_number?: number;
  price?: number;
  tax?: number;
  po_quote_number?: string;
  deleted_reason?: string;
  canceled_reason?: string;
  is_complete?: boolean;
  is_canceled?: boolean;
  completed_on?: Date;
  completed_by?: number;
  canceled_on?: Date;
  canceled_by?: number;
  purchase_order_lines?: PurchaseOrderLineEditCommand[];
}

export class PurchaseOrderLineCreateCommand extends DataCommand {
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

  product_name: string = ""; // This field is not used except in the UI
}

export class PurchaseOrderLineEditCommand extends DataCommand {
  id?: number;
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
}

export class PurchaseOrderHeaderDeleteCommand extends DataCommand {
  id?: number;
}

export class PurchaseOrderHeaderFindCommand extends DataCommand {
  vendor_id?: number;
  wildcard?: string;
}

export class PurchaseOrderLineDeleteCommand extends DataCommand {
  id?: number;
}

