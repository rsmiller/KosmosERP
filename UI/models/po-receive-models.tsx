import { BaseDto, DataCommand } from "./base-models";
import { DocumentUploadDto } from "./document-models";

export class PurchaseOrderReceiveHeaderDto extends BaseDto {
  purchase_order_id?: number;
  units_ordered?: number;
  units_received?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  completed_on?: Date;
  canceled_on?: Date;
  canceled_by?: number;
  received_lines?: PurchaseOrderReceiveLineDto[];
  received_uploads?: PurchaseOrderReceiveUploadDto[];

  po_number?: number;
  po_by?: string;
}

export class PurchaseOrderReceiveLineDto extends BaseDto {
  purchase_order_receive_header_id?: number;
  purchase_order_line_id?: number;
  units_ordered?: number;
  units_received?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  completed_on?: Date;
  canceled_on?: Date;
  canceled_by?: number;

  product_name?: string; // for ui
  line_number?: number = 0;
}

export class PurchaseOrderReceiveUploadDto extends BaseDto
{
    purchase_order_receive_header_id?: number;
    document_upload_id?: number;

    document_upload?: DocumentUploadDto;
}

export class PurchaseOrderReceiveHeaderCreateCommand extends DataCommand {
  purchase_order_id?: number;
  document_upload_id?: number;
  units_ordered?: number;
  units_received?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  completed_on?: string;
  canceled_on?: string;
  canceled_by?: number;
  received_lines?: PurchaseOrderReceiveLineCreateCommand[];
  received_uploads?: PurchaseOrderReceiveUploadCreateCommand[];
}

export class PurchaseOrderReceiveHeaderEditCommand extends DataCommand {
  id?: number;
  purchase_order_id?: number;
  units_ordered?: number;
  units_received?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  completed_on?: string;
  canceled_on?: string;
  canceled_by?: number;
  received_lines?: PurchaseOrderReceiveLineEditCommand[];
  received_uploads?: PurchaseOrderReceiveUploadEditCommand[];
}

export class PurchaseOrderReceiveLineCreateCommand extends DataCommand {
  purchase_order_receive_header_id?: number;
  purchase_order_line_id?: number;
  units_ordered?: number;
  units_received?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  completed_on?: string;
  canceled_on?: string;
  canceled_by?: number;

  product_name?: string; // for ui
  units_already_received?: number;  // for ui
}

export class PurchaseOrderReceiveLineEditCommand extends DataCommand {
  id?: number;
  purchase_order_receive_header_id?: number;
  purchase_order_line_id?: number;
  units_ordered?: number;
  units_received?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  completed_on?: string;
  canceled_on?: string;
  canceled_by?: number;
}

export class PurchaseOrderReceiveUploadCreateCommand extends DataCommand {
  purchase_order_receive_header_id?: number;
  document_upload_id?: number;
}

export class PurchaseOrderReceiveUploadEditCommand extends DataCommand {
  id?: number;
  purchase_order_receive_header_id?: number;
  document_upload_id?: number;
}

export class PurchaseOrderReceiveHeaderListDto extends BaseDto {
  purchase_order_id?: number;
  units_ordered?: number;
  units_received?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  completed_on?: Date;
  canceled_on?: Date;
  canceled_by?: number;
  po_number?: number;

}

export class PurchaseOrderReceiveHeaderDeleteCommand extends DataCommand {
  id?: number;
}

export class PurchaseOrderReceiveHeaderFindCommand extends DataCommand {
  purchase_order_id?: number;
  wildcard?: string;
}

export class PurchaseOrderReceiveLineDeleteCommand extends DataCommand {
  id?: number;
}


export class POReceiveAGGridDTO
{
    purchase_order_id?: number;
    purchase_order_line_id?: number;
    line_number?: number;

    product_name?: string;

    total_units_ordered: number = 0;
    total_units_received: number = 0;
    
    units_to_receive: number = 0;
    max_to_receive: number = 0;

    all_received_lines?: PurchaseOrderReceiveLineDto[] = new Array<PurchaseOrderReceiveLineDto>();
    all_received_uploads?: PurchaseOrderReceiveUploadDto[] = new Array<PurchaseOrderReceiveUploadDto>();
}