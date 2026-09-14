import { ARInvoiceLineDto } from "./ar-models";
import { BaseDto, DataCommand } from "./base-models";
import { PurchaseOrderReceiveLineDto } from "./po-receive-models";
import { PurchaseOrderLineDto } from "./purchase-order-models";
import { OrderLineDto } from "./sales-order-models";

export class APInvoiceHeaderDto implements BaseDto {
  id: number = 0;
  is_deleted?: boolean | undefined;
  created_on?: string | undefined;
  created_by?: string | undefined;
  created_on_timezone?: string | undefined;
  created_on_string?: string | undefined;
  updated_on?: string | null | undefined;
  updated_by?: string | null | undefined;
  updated_on_timezone?: string | null | undefined;
  updated_on_string?: string | null | undefined;
  deleted_on?: string | null | undefined;
  deleted_by?: string | null | undefined;
  deleted_on_timezone?: string | null | undefined;
  deleted_on_string?: string | null | undefined;
  guid?: string | null | undefined;
  vendor_id?: number;
  invoice_number?: string;
  invoice_date?: Date;
  invoice_due_date?: Date;
  invoice_received_date?: Date;
  invoice_total?: number;
  memo?: string;
  purchase_order_receive_id?: number;
  association_object_id?: number;
  association_number?: number;
  association_is_purchase_order?: boolean;
  association_is_sales_order?: boolean;
  association_is_ar_invoice?: boolean;
  packing_list_is_required?: boolean;
  is_paid?: boolean;

  vendor_name?: string;
  first_po_receive_date?: Date;

  ap_invoice_lines?: APInvoiceLineDto[];
  po_lines?: PurchaseOrderLineDto[];
  order_lines?: OrderLineDto[];
  ar_lines?: ARInvoiceLineDto[];
  receive_lines?: PurchaseOrderReceiveLineDto[];
}

export class APInvoiceHeaderListDto implements BaseDto {
  id: number = 0;
  is_deleted?: boolean | undefined;
  created_on?: string | undefined;
  created_by?: string | undefined;
  created_on_timezone?: string | undefined;
  created_on_string?: string | undefined;
  updated_on?: string | null | undefined;
  updated_by?: string | null | undefined;
  updated_on_timezone?: string | null | undefined;
  updated_on_string?: string | null | undefined;
  deleted_on?: string | null | undefined;
  deleted_by?: string | null | undefined;
  deleted_on_timezone?: string | null | undefined;
  deleted_on_string?: string | null | undefined;
  guid?: string | null | undefined;
  vendor_id?: number;
  invoice_number?: string;
  invoice_date?: Date;
  invoice_due_date?: Date;
  invoice_received_date?: Date;
  invoice_total?: number;
  memo?: string;
  packing_list_is_required?: boolean;
  is_paid?: boolean;
  vendor_name?: string;
}

export class APInvoiceLineDto implements BaseDto {
  id: number = 0;
  is_deleted?: boolean | undefined;
  created_on?: string | undefined;
  created_by?: string | undefined;
  created_on_timezone?: string | undefined;
  created_on_string?: string | undefined;
  updated_on?: string | null | undefined;
  updated_by?: string | null | undefined;
  updated_on_timezone?: string | null | undefined;
  updated_on_string?: string | null | undefined;
  deleted_on?: string | null | undefined;
  deleted_by?: string | null | undefined;
  deleted_on_timezone?: string | null | undefined;
  deleted_on_string?: string | null | undefined;
  guid?: string | null | undefined;
  ap_invoice_header_id?: number;
  line_number?: number;
  line_total?: number;
  qty_invoiced?: number;
  units_ordered?: number;
  units_received?: number;
  gl_account?: string;
  description?: string;
  association_object_id?: number;
  association_object_line_id?: number;
  association_is_purchase_order?: boolean;
  association_is_sales_order?: boolean;
  association_is_ar_invoice?: boolean;
  receive_lines?: PurchaseOrderReceiveLineDto[] = new Array<PurchaseOrderReceiveLineDto>();
}

export class APInvoiceLineListDto implements BaseDto {
  id: number = 0;
  is_deleted?: boolean | undefined;
  created_on?: string | undefined;
  created_by?: string | undefined;
  created_on_timezone?: string | undefined;
  created_on_string?: string | undefined;
  updated_on?: string | null | undefined;
  updated_by?: string | null | undefined;
  updated_on_timezone?: string | null | undefined;
  updated_on_string?: string | null | undefined;
  deleted_on?: string | null | undefined;
  deleted_by?: string | null | undefined;
  deleted_on_timezone?: string | null | undefined;
  deleted_on_string?: string | null | undefined;
  guid?: string | null | undefined;
  ap_invoice_header_id?: number;
  line_number?: number;
  line_total?: number;
  qty_invoiced?: number;
  units_ordered?: number;
  units_received?: number;
  gl_account?: string;
  description?: string;
  association_object_id?: number;
  association_object_line_id?: number;
}

export class APInvoiceAssociationDto {
  id?: number;
  identifier?: string;
  is_purchase_order?: boolean;
  is_sales_order?: boolean;
  is_ar_invoice?: boolean;
  additional_data?: { [key: string]: string };
}

export class APInvoiceHeaderCreateCommand extends DataCommand {
  vendor_id: number = 0;

  invoice_number?: string = "";

  invoice_date?: Date;

  invoice_due_date?: Date;

  invoice_received_date?: Date;

  invoice_total: number = 0;

  memo?: string;

  purchase_order_receive_id?: number

  packing_list_is_required: boolean = false;

  association_object_id?: number;

  association_is_purchase_order: boolean = false;

  association_is_sales_order: boolean = false;

  association_is_ar_invoice: boolean = false;

  is_paid: boolean = false;

  ap_invoice_lines: APInvoiceLineCreateCommand[] = new Array<APInvoiceLineCreateCommand>();
}

export class APInvoiceLineCreateCommand extends DataCommand {
  ap_invoice_header_id?: number;

  line_number: number = 0;

  line_total: number = 0;

  qty_invoiced: number = 0;

  gl_account: string = "";

  description: string = "";

  association_object_id?: number;

  association_object_line_id?: number;

  association_is_purchase_order: boolean = false;

  association_is_sales_order: boolean = false;

  association_is_ar_invoice: boolean = false;

    //UI
  qty_ordered?: number = 0;
  max_quantity: number = 0;
  total_invoiced: number = 0;
}

export class APInvoiceHeaderEditCommand extends DataCommand {
  id: number = 0;
  invoice_number?: string;
  vendor_id?: number;
  invoice_date?: Date;
  invoice_due_date?: Date;
  invoice_received_date?: Date;
  invoice_total?: number;
  memo?: string;
  purchase_order_receive_id?: number;
  association_object_id?: number;
  association_is_purchase_order?: boolean;
  association_is_sales_order?: boolean;
  association_is_ar_invoice?: boolean;
  packing_list_is_required?: boolean;
  is_paid?: boolean;
  //UI
  qty_ordered?: number = 0;
  ap_edit_invoice_lines: APInvoiceLineEditCommand[] = new Array<APInvoiceLineEditCommand>(); // Required, default empty array
  ap_create_invoice_lines: APInvoiceLineCreateCommand[] = new Array<APInvoiceLineCreateCommand>(); // Required, default empty array
}

export class APInvoiceLineEditCommand extends DataCommand {
  id: number = 0;
  line_number?: number;
  line_total?: number;
  qty_invoiced?: number;
  gl_account?: string;
  description: string = "";
  purchase_order_receive_line_id?: number;
  association_object_id?: number;
  association_object_line_id?: number; 
  association_is_purchase_order?: boolean;
  association_is_sales_order?: boolean;
  association_is_ar_invoice?: boolean;

  //UI
  qty_ordered?: number = 0;
  max_quantity: number = 0;
  total_invoiced: number = 0;
}

export class APInvoiceHeaderDeleteCommand extends DataCommand {
  id: number = 0;
}

export class APInvoiceHeaderFindCommand extends DataCommand {
  vendor_id?: number;
  invoice_number?: string;
  invoice_date?: string;
  invoice_due_date?: string;
  invoice_received_date?: string;
  invoice_total?: number;
  memo?: string;
  packing_list_is_required?: boolean;
  is_paid?: boolean;
  vendor_name?: string;
}

export class APInvoiceLineDeleteCommand extends DataCommand {
  id: number = 0;
}

export class APInvoiceAssoicationCommand extends DataCommand {
  ap_invoice_header_id?: number;
  association_object_id?: number;
  association_is_purchase_order?: boolean;
  association_is_sales_order?: boolean;
  association_is_ar_invoice?: boolean;
}

export class APInvoiceAssociatePOCommand extends DataCommand {
  ap_invoice_line_id?: number;
  purchase_order_receive_line_id?: number;
}

export class UIAssociatedLines
{
  id?: number;
  line_number?: number;
  description?: string;
  units?: number = 0;
  is_purchase_order?: boolean;
  is_sales_order?: boolean;
  is_ar_invoice?: boolean;
  
}

