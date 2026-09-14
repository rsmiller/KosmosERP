import { AddressDto } from "./address-models";
import { BaseDto, DataCommand } from "./base-models";

export class ShipmentLineDto extends BaseDto {
    shipment_header_id?: number;
    order_line_id?: number;
    units_to_ship?: number;
    units_ordered?: number;
    units_shipped?: number;
    is_complete?: boolean;
    is_canceled?: boolean;
    completed_on?: Date | null;
    completed_by?: number | null;
    canceled_on?: Date | null;
    canceled_by?: number | null;
    canceled_reason?: string | null;

    line_description?: string | null;
    line_number?: number;
    identifier1?: string;
}

export class ShipmentLineListDto extends BaseDto {
    shipment_header_id?: number;
    order_line_id?: number;
    units_to_ship?: number;
    units_ordered?: number;
    units_shipped?: number;
    is_complete?: boolean;
    is_canceled?: boolean;
    completed_on?: Date | null;
    completed_by?: number | null;
    canceled_on?: Date | null;
    canceled_by?: number | null;
    canceled_reason?: string | null;
}

export class ShipmentHeaderDto extends BaseDto {
    order_header_id?: number;
    shipment_number?: number;
    address_id?: number;
    units_to_ship?: number;
    units_shipped?: number;
    is_complete?: boolean;
    is_canceled?: boolean;
    is_released?: boolean;
    ship_via?: string;
    ship_attn?: string | null;
    freight_carrier?: string | null;
    freight_charge_amount?: number;
    tax?: number;
    completed_on?: Date | null;
    completed_by?: number | null;
    canceled_on?: Date | null;
    canceled_by?: number | null;
    canceled_reason?: string | null;

    shipment_lines?: ShipmentLineDto[];
    address?: AddressDto;

    customer_name?: string;
    po_number?: string;
    order_date?: string;
    order_number?: number;
}

export class ShipmentHeaderListDto extends BaseDto {
    order_header_id?: number;
    shipment_number?: number;
    address_id?: number;
    units_to_ship?: number;
    units_shipped?: number;
    is_complete?: boolean;
    is_canceled?: boolean;
    is_released?: boolean;
    ship_via?: string;
    ship_attn?: string | null;
    freight_carrier?: string | null;
    freight_charge_amount?: number;
    tax?: number;
    completed_on?: Date | null;
    completed_by?: number | null;
    canceled_on?: Date | null;
    canceled_by?: number | null;
    canceled_reason?: string | null;
    freight_carrier_name?: string;
    
    address?: AddressDto;

    shipment_lines?: ShipmentLineListDto[];
}

export class ShipmentHeaderCreateCommand extends DataCommand {
  order_header_id?: number;
  address_id?: number;
  ship_via?: string;
  ship_attn?: string;
  freight_carrier?: string;
  freight_charge_amount?: number;
  tax?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;
  shipment_lines?: ShipmentLineCreateCommand[];
}

export class ShipmentHeaderEditCommand extends DataCommand{
  id?: number;
  order_id?: number | null;
  address_id?: number | null;
  ship_via?: string | null;
  ship_attn?: string | null;
  freight_carrier?: string | null;
  freight_charge_amount?: number | null;
  tax?: number | null;
  is_complete?: boolean | null;
  is_canceled?: boolean | null;
  is_released?: boolean | null;
  canceled_reason?: string | null;

  shipment_lines?: ShipmentLineEditCommand[];
}

export class ShipmentLineCreateCommand extends DataCommand {
  order_line_id?: number;
  units_to_ship?: number;
  units_shipped?: number;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_reason?: string;

  line_description?: string;
  line_number?: number;
  units_ordered?: number;
  units_already_shipped?: number;
}

export class ShipmentLineEditCommand extends DataCommand {
  id?: number;
  order_line_id?: number | null;
  units_to_ship?: number | null;
  units_shipped?: number | null;
  is_complete?: boolean | null;
  is_canceled?: boolean | null;
  canceled_reason?: string | null;
}

export class ShipmentHeaderDeleteCommand extends DataCommand {
  id?: number;
}

export class ShipmentHeaderFindCommand extends DataCommand {
  wildcard?: string;
  order_guid?: string;
}

export class ShipmentLineDeleteCommand extends DataCommand {
  id?: number;
}

export class vw_ReadyToShip
{
  order_number?: number;
  order_guid?: string;
  customer_name?: string;
  product_name?: string;
  sold_quantity?: number = 0;
  produced_quantity?: number = 0;
  shipped_quantity?: number = 0;
}
