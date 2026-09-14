import { BaseDto, DataCommand } from "./base-models";
import { BOMListDto } from "./bom-models";
import { OrderHeaderDto, OrderLineDto } from "./sales-order-models";

export class ProductionOrderHeaderDto extends BaseDto {
    order_header_id?: number;
    status?: string;
    priority_id?: number;
    planned_start_date?: Date | null;
    planned_complete_date?: Date | null;
    actual_completed_on?: Date | null;
    is_complete?: boolean;
    production_lead_minutes?: number;

    production_order_lines?: ProductionOrderLineDto[];
    order_header?: OrderHeaderDto;

    status_name?: string;
}

export class ProductionOrderHeaderListDto extends BaseDto {
    order_header_id?: number;
    status?: string;
    priority_id?: number;
    planned_start_date?: Date;
    planned_complete_date?: Date;
    actual_completed_on?: Date;
    is_complete?: boolean;
    production_lead_minutes?: number;

    status_name?: string;
    order_number?: number;
}

export class ProductionOrderLineDto extends BaseDto {
    production_order_header_id?: number;
    order_line_id?: number;
    quantity?: number;
    status?: string;
    started_on?: Date;
    completed_on?: Date;
    is_complete?: boolean;
    production_lead_minutes?: number;

    order_line?: OrderLineDto;
    boms?: BOMListDto[];    
    
    status_name?: string;
    order_number?: number;
}

export class ProductionOrderHeaderCreateCommand extends DataCommand {
    order_header_id?: number;
    status?: string;
    priority_id?: number;
    planned_start_date?: string;
    planned_complete_date?: string;
    actual_completed_on?: string;
    is_complete?: boolean;
    production_lead_minutes?: number;
    production_order_lines?: ProductionOrderLineCreateCommand[];
}

export class ProductionOrderHeaderEditCommand extends DataCommand {
    id?: number;
    order_header_id?: number;
    status?: string;
    priority_id?: number;
    planned_start_date?: Date;
    planned_complete_date?: Date;
    actual_completed_on?: Date;
    is_complete?: boolean;
    production_lead_minutes?: number;
    production_order_lines?: ProductionOrderLineEditCommand[];
}

export class ProductionOrderLineCreateCommand extends DataCommand {
    production_order_header_id?: number;
    order_line_id?: number;
    quantity?: number;
    status?: string;
    started_on?: string;
    completed_on?: string;
    is_complete?: boolean;
    production_lead_minutes?: number;
}

export class ProductionOrderLineEditCommand extends DataCommand {
    id?: number;
    production_order_header_id?: number;
    order_line_id?: number;
    quantity?: number;
    status?: string;
    started_on?: string;
    completed_on?: string;
    is_complete?: boolean;
    production_lead_minutes?: number;
}

export class ProductionOrderHeaderDeleteCommand extends DataCommand {
    id?: number;
}

export class ProductionOrderHeaderFindCommand extends DataCommand {
    status_name?: string;
    wildcard?: string;
}

export class ProductionOrderLineDeleteCommand extends DataCommand {
    id?: number;
}
